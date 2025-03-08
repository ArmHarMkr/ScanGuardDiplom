using HtmlAgilityPack;
using MGOBankApp.BLL.Interfaces;
using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Models;
using MGOBankApp.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace MGOBankApp.Service.Implementations
{
    public class ScannerService : IScannerService
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IVulnerabilityAnalyzer _analyzer;
        private readonly ILogger<ScannerService> _logger;

        public ScannerService(HttpClient httpClient, IVulnerabilityAnalyzer analyzer, UserManager<ApplicationUser> userManager, ApplicationDbContext context,ILogger<ScannerService> logger)
        {
            _userManager = userManager;
            _analyzer = analyzer;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _context = context;
            _logger = logger;
        }

        public async Task<Vulnerability> ScanUrl(string url, ApplicationUser? applicationUser)
        {
            try
            {
                Vulnerability vulnerability = new Vulnerability();
                var baseUri = new Uri(url);
                _httpClient.BaseAddress = new Uri(baseUri.GetLeftPart(UriPartial.Authority));

                // Получаем HTML страницы
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Ошибка доступа к сайту: {response.StatusCode}");

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Ищем формы
                var forms = doc.DocumentNode.SelectNodes("//form") ?? new HtmlNodeCollection(null);

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

                    vulnerability.SQLi = await _analyzer.SqlInjectionTest(data, method, absoluteAction);
                    vulnerability.XSS = await _analyzer.XssTest(method, absoluteAction, data);
                    vulnerability.CSRF = _analyzer.CsrfTest(method, form);
                }

                // Логирование
                _logger.LogInformation("{user} просканировал URL - {url} ", applicationUser!.Email, url);
                return vulnerability;
            }
            catch (Exception ex)
            {
                _logger.LogError("{user} : Ошибка при сканировании URL - {url} ... {exeptionMessage}", applicationUser!.Email!, url, ex.Message);
                throw new Exception($"Ошибка сканирования: {ex.Message}");
            }
        }






    }
}