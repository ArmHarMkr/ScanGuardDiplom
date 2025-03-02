using HtmlAgilityPack;
using MGOBankApp.BLL.Interfaces;
using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Models;
using MGOBankApp.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MGOBankApp.Service.Implementations
{
    public class ScannerService : IScannerService
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IVulnerabilityAnalyzer _analyzer;

        public ScannerService(HttpClient httpClient, IVulnerabilityAnalyzer analyzer, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _analyzer = analyzer;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _context = context;
        }

        public async Task<Vulnerability> ScanUrl(string url, ApplicationUser? applicationUser)
        {
            try
            {
                Vulnerability vulnerability = new Vulnerability();
                var baseUri = new Uri(url);
                _httpClient.BaseAddress = new Uri(baseUri.GetLeftPart(UriPartial.Authority));


                if (_context.SiteScanCounts.Any(x => x.Url == _httpClient.BaseAddress.ToString()))
                {
                    var siteScanCount = await _context.SiteScanCounts.FirstOrDefaultAsync(x => x.Url == _httpClient.BaseAddress.ToString());
                    siteScanCount.CheckCount++;
                }
                else
                {
                    SiteScanCountEntity siteScanCount = new SiteScanCountEntity()
                    {
                        Url = _httpClient.BaseAddress.ToString(),
                        CheckCount = 1
                    };
                    _context.SiteScanCounts.Add(siteScanCount);
                }

                await _context.SaveChangesAsync();


                // Check if the URL uses HTTPS
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Ошибка доступа к сайту : {response.StatusCode}");

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var forms = doc.DocumentNode.SelectNodes("//form") ?? new HtmlNodeCollection(null);



                vulnerability.HTTPWithoutS = _analyzer.IsHttp(baseUri);


                foreach (var form in forms)
                {
                    var action = form.GetAttributeValue("action", url);
                    var method = form.GetAttributeValue("method", "get").ToLower();
                    var inputs = form.SelectNodes(".//input");

                    var absoluteAction = action.StartsWith("http") ? action : new Uri(baseUri, action).ToString();
                    var data = new Dictionary<string, string>();
                    if (inputs != null)
                    {
                        foreach (var input in inputs)
                        {
                            var name = input.GetAttributeValue("name", null);
                            if (!string.IsNullOrEmpty(name))
                                data[name] = "";
                        }
                    }

                    // SQL Injection Test    
                    vulnerability.SQLi = await _analyzer.SqlInjectionTest(data, method, absoluteAction);

                    // XSS Test
                    vulnerability.XSS = await _analyzer.XssTest(method, absoluteAction, data);
                    // CSRF Test
                    vulnerability.CSRF = _analyzer.CsrfTest(method, form);
                }

                int vulnCount = _analyzer.calculateVulnerabilityCount(vulnerability);
                if (applicationUser != null)
                {
                    WebsiteScanEntity websiteScanEntity = new()
                    {
                        ScanUser = applicationUser,
                        Status = "Scanned",
                        Url = url,
                        VulnerablityCount = vulnCount
                    };

                    _context.WebsiteScanEntities.Add(websiteScanEntity);

                    if (vulnerability.SQLi)
                    {
                        _context.VulnerabilityEntities.Add(new VulnerabilityEntity
                        {
                            ScanEntity = websiteScanEntity,
                            VulnerabilityType = Domain.Enums.VulnerabilityType.SQLi
                        });
                    }
                    if (vulnerability.XSS)
                    {
                        _context.VulnerabilityEntities.Add(new VulnerabilityEntity
                        {
                            ScanEntity = websiteScanEntity,
                            VulnerabilityType = Domain.Enums.VulnerabilityType.XSS
                        });
                    }
                    if (vulnerability.CSRF)
                    {
                        _context.VulnerabilityEntities.Add(new VulnerabilityEntity
                        {
                            ScanEntity = websiteScanEntity,
                            VulnerabilityType = Domain.Enums.VulnerabilityType.CSRF
                        });
                    }
                    if (vulnerability.HTTPWithoutS)
                    {
                        _context.VulnerabilityEntities.Add(new VulnerabilityEntity
                        {
                            ScanEntity = websiteScanEntity,
                            VulnerabilityType = Domain.Enums.VulnerabilityType.HTTPWithoutS
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                return vulnerability;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сканирования: {ex.Message}");
            }
        }


    }
}