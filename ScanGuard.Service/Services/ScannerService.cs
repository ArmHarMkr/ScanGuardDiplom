using HtmlAgilityPack;
using ScanGuard.BLL.Interfaces;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using ScanGuard.Domain.Enums;
using ScanGuard.Models;
using ScanGuard.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace ScanGuard.Service.Implementations
{
    public class ScannerService : IScannerService
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IVulnerabilityAnalyzer _analyzer;
        private readonly ILogger<ScannerService> _logger;

        public ScannerService(
            HttpClient httpClient,
            IVulnerabilityAnalyzer analyzer,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<ScannerService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _analyzer = analyzer;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<(Vulnerability vulnerability, 
                           Dictionary<int, (bool IsOpen, string Service, string Version)> portResults)> ScanUrl(string url, ApplicationUser? applicationUser)
        {
            try
            {
                Vulnerability vulnerability = new Vulnerability();
                var baseUri = new Uri(url);
                _httpClient.BaseAddress = new Uri(baseUri.GetLeftPart(UriPartial.Authority));

                // Get HTML page
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Error : Acces to this site denied: {response.StatusCode}");

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Scan ports
                var portResults = await _analyzer.ScanPorts(baseUri.Host);

                // Check forms
                var forms = doc.DocumentNode.SelectNodes("//form") ?? new HtmlNodeCollection(null);

                foreach (var form in forms)
                {
                    var action = form.GetAttributeValue("action", url);
                    var method = form.GetAttributeValue("method", "get").ToLower();
                    var absoluteAction = action.StartsWith("http") ? action : new Uri(baseUri, action).ToString();
                    var data = new Dictionary<string, string>();

                    var inputFields = form.SelectNodes(".//input");
                    if (inputFields != null)
                    {
                        foreach (var input in inputFields)
                        {
                            var name = input.GetAttributeValue("name", null);
                            if (!string.IsNullOrEmpty(name))
                                data[name] = "";
                        }
                    }

                    // Perform vulnerability tests
                    vulnerability.SQLi = await _analyzer.SqlInjectionTest(url);
                    vulnerability.XSS = await _analyzer.XssTest(method, absoluteAction, data);
                    vulnerability.CSRF = _analyzer.CsrfTest(method, form);
                    vulnerability.HTTPWithoutS = _analyzer.IsHttp(baseUri);
                    vulnerability.Phishing = await _analyzer.PhishingTest(url);
                    vulnerability.RFI = await _analyzer.RfiTest(url);
                    vulnerability.LFI = await _analyzer.LfiTest(url);
                    vulnerability.HTTPResponseSplitting = await _analyzer.HttpResponseSplittingTest(url);
                    vulnerability.IDOR = await _analyzer.IdorTest(url);
                    vulnerability.SecurityMisconfiguration = await _analyzer.SecurityMisconfigurationTest(url);
                    vulnerability.UnvalidatedRedirectAndForwards = await _analyzer.UnvalidatedRedirectTest(url);
                    vulnerability.DirectoryListing = await _analyzer.DirectoryListingTest(url);
                    vulnerability.BrokenAuthentification = await _analyzer.BrokenAuthenticationTest(url);
                    vulnerability.CheckWeakPasswordPolicy = await _analyzer.CheckWeakPasswordPolicy(url);
                }

                // Save scan results to database
                WebsiteScanEntity websiteScanEntity = new()
                {
                    Status = "Scanned",
                    ScanUser = applicationUser,
                    Url = url,
                    VulnerablityCount = _analyzer.CalculateVulnerabilityCount(vulnerability)
                };

                List<VulnerabilityEntity> vulnerabilities = new();
                AddVulnerabilitiesToList(vulnerability, websiteScanEntity, vulnerabilities);

                _context.WebsiteScanEntities.Add(websiteScanEntity);
                _context.VulnerabilityEntities.AddRange(vulnerabilities);
                await _context.SaveChangesAsync();

                // Logging
                    _logger.LogInformation("{user} Scanned URL - {url} ", applicationUser?.Email, url);


                return (vulnerability, portResults);
            }
            catch (Exception ex)
            {
                _logger.LogError("{user} : URL Scanning error - {url} ... {exeptionMessage}",
                    applicationUser?.Email ?? "Unknown", url, ex.Message);
                throw new Exception($"Scanning Error: {ex.Message}");
            }
        }

        private void AddVulnerabilitiesToList(Vulnerability vulnerability,
            WebsiteScanEntity websiteScanEntity,
            List<VulnerabilityEntity> vulnerabilities)
        {
            if (vulnerability.SQLi)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.SQLi });
            if (vulnerability.XSS)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.XSS });
            if (vulnerability.CSRF)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.CSRF });
            if (vulnerability.HTTPWithoutS)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.HTTPWithoutS });
            if (vulnerability.Phishing)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.Phishing });
            if (vulnerability.RFI)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.RFI });
            if (vulnerability.LFI)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.LFI });
            if (vulnerability.HTTPResponseSplitting)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.HTTPResponseSplitting });
            if (vulnerability.IDOR)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.IDOR });
            if (vulnerability.SecurityMisconfiguration)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.SecurityMisconfiguration });
            if (vulnerability.UnvalidatedRedirectAndForwards)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.UnvalidatedRedirectAndForwards });
            if (vulnerability.DirectoryListing)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.DirectoryListing });
            if (vulnerability.BrokenAuthentification)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.BrokenAuthentification });
            if (vulnerability.CheckWeakPasswordPolicy)
                vulnerabilities.Add(new VulnerabilityEntity { ScanEntity = websiteScanEntity, VulnerabilityType = VulnerabilityType.CheckWeakPasswordPolicy });
        }
    }
}
