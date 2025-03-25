using HtmlAgilityPack;
using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Domain.Enums;
using MGOBankApp.Models;
using MGOBankApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MGOBankAp.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly SignInManager<ApplicationUser> SignInManager;
        private readonly ApplicationDbContext Context;

        public HomeController(IHttpClientFactory httpClientFactory, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
                              ApplicationDbContext db)
        {
            _httpClient = httpClientFactory.CreateClient();
            UserManager = userManager;
            SignInManager = signInManager;
            Context = db;
        }

        public async Task<IActionResult> Index()
        {
            bool isSignedIn = SignInManager.IsSignedIn(User);
            int siteCount = isSignedIn ? 30 : 10;

            List<WebsiteScanEntity> mostScanCount = await Context.WebsiteScanEntities
                .OrderByDescending(x => x.VulnerablityCount)
                .Take(siteCount)
                .ToListAsync();

            // Get local IP
            string userIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // Check if behind a proxy (get real client IP)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                userIpAddress = Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }

            // Get public IP (only if the IP is private)
            if (userIpAddress.StartsWith("10.") || userIpAddress.StartsWith("192.168.") || userIpAddress.StartsWith("172.16.") || userIpAddress == "::1")
            {
                userIpAddress = await GetPublicIp();
            }

            ViewBag.UserIp = userIpAddress;

            return View(mostScanCount);
        }

        private async Task<string> GetPublicIp()
        {
            try
            {
                using var client = new HttpClient();
                return await client.GetStringAsync("https://api4.ipify.org"); // Fetches IPv4 only
            }
            catch
            {
                return "Unable to retrieve public IPv4";
            }
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult FwdScanner()
        {
            return RedirectToAction("Scanner");
        }

        [HttpGet]
        public IActionResult Scanner()
        {
            return View(new Vulnerability());
        }

        [HttpPost]
        public async Task<IActionResult> Scanner(string url)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("URL не указан");

            try
            {
                var baseUri = new Uri(url);
                _httpClient.BaseAddress = new Uri(baseUri.GetLeftPart(UriPartial.Authority));

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, "Ошибка доступа к сайту");

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var forms = doc.DocumentNode.SelectNodes("//form") ?? new HtmlNodeCollection(null);
                Vulnerability vulnerability = new Vulnerability();

                int vulnCount = 0;
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

                    // Test SQLi
                    var normalPayload = "test";
                    foreach (var key in data.Keys)
                        data[key] = normalPayload;

                    var (normalResponse, normalStatusCode) = await SendRequest(method, absoluteAction, data);

                    var sqliPayload = "' OR 1=1 --";
                    foreach (var key in data.Keys)
                        data[key] = sqliPayload;

                    var (sqliResponse, sqliStatusCode) = await SendRequest(method, absoluteAction, data);

                    if (sqliStatusCode >= 200 && sqliStatusCode < 300 || normalStatusCode != sqliStatusCode)
                    {
                        vulnerability.SQLi = true;
                        vulnCount++;
                    }
                    else if (sqliStatusCode >= 400 && normalStatusCode < 500)
                    {
                        vulnerability.SQLi = true;
                        vulnCount++;
                    }

                    // Test XSS
                    var xssPayload = "<script>alert('xss')</script>";
                    foreach (var key in data.Keys)
                        data[key] = xssPayload;

                    var (xssResponse, _) = await SendRequest(method, absoluteAction, data);
                    if (xssResponse.Contains(xssPayload))
                    {
                        vulnerability.XSS = true;
                        vulnCount++;
                    }

                    // Test CSRF
                    var csrfToken = form.SelectSingleNode(".//input[@name='csrf_token']");
                    if (csrfToken == null && method == "post")
                    {
                        vulnerability.CSRF = true;
                        vulnCount++;
                    }
                }

                // Update SiteScanCountEntity
                var siteScanCount = await Context.SiteScanCounts.FirstOrDefaultAsync(s => s.Url == url);
                if (siteScanCount == null)
                {
                    siteScanCount = new SiteScanCountEntity { Url = url, CheckCount = 1 };
                    Context.SiteScanCounts.Add(siteScanCount);
                    await Context.SaveChangesAsync();
                }
                else
                {
                    siteScanCount.CheckCount++;
                    await Context.SaveChangesAsync();
                }

                if (SignInManager.IsSignedIn(User))
                {
                    var currentUser = await UserManager.GetUserAsync(User);
                    if (currentUser == null)
                    {
                        return BadRequest("No user found");
                    }

                    WebsiteScanEntity websiteScanEntity = new()
                    {
                        ScanUser = currentUser,
                        Url = url,
                        Status = "Scanned",
                        VulnerablityCount = vulnCount,
                    };
                    VulnerabilityEntity sqli = new()
                    {
                        ScanEntity = websiteScanEntity,
                        VulnerabilityType = VulnerabilityType.SQLi
                    };
                    VulnerabilityEntity xss = new()
                    {
                        ScanEntity = websiteScanEntity,
                        VulnerabilityType = VulnerabilityType.XSS
                    };
                    VulnerabilityEntity csrf = new()
                    {
                        ScanEntity = websiteScanEntity,
                        VulnerabilityType = VulnerabilityType.CSRF
                    };

                    Context.Add(websiteScanEntity);
                    Context.Add(sqli);
                    Context.Add(xss);
                    Context.Add(csrf);
                }

                await Context.SaveChangesAsync();

                return View("Scanner", vulnerability);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        private async Task<(string response, int statusCode)> SendRequest(string method, string action, Dictionary<string, string> data)
        {
            HttpResponseMessage response;
            if (method == "post")
            {
                var content = new FormUrlEncodedContent(data);
                response = await _httpClient.PostAsync(action, content);
            }
            else
            {
                var query = string.Join("&", data.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                response = await _httpClient.GetAsync($"{action}?{query}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return (responseBody, (int)response.StatusCode);
        }
    }
}