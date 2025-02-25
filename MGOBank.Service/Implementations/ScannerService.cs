using HtmlAgilityPack;
using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Models;
using MGOBankApp.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.Service.Implementations
{
    public class ScannerService : IScannerService
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ScannerService(HttpClient httpClient, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _context = context;
        }

        public async Task<Vulnerability> ScanUrl(string url, ApplicationUser? applicationUser)
        {
            try
            {
                var baseUri = new Uri(url);
                _httpClient.BaseAddress = new Uri(baseUri.GetLeftPart(UriPartial.Authority));

                // Check if the URL uses HTTPS
                bool isHttps = baseUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Ошибка доступа к сайту : {response.StatusCode}");

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var forms = doc.DocumentNode.SelectNodes("//form") ?? new HtmlNodeCollection(null);
                Vulnerability vulnerability = new Vulnerability
                {
                    HTTPWithoutS = !isHttps
                };

                int vulnCount = 0;

                // Count HTTP vulnerability separately
                if (vulnerability.HTTPWithoutS)
                {
                    vulnCount++;
                }

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
                    }

                    // XSS Test
                    var xssPayload = "<script>alert('xss')</script>";
                    foreach (var key in data.Keys)
                        data[key] = xssPayload;

                    var (xssResponse, _) = await SendRequest(method, absoluteAction, data);
                    if (xssResponse.Contains(xssPayload))
                    {
                        vulnerability.XSS = true;
                    }

                    // CSRF Test
                    var csrfToken = form.SelectSingleNode(".//input[@name='csrf_token']");
                    if (csrfToken == null && method == "post")
                    {
                        vulnerability.CSRF = true;
                    }
                }

                if (vulnerability.SQLi) vulnCount++;
                if (vulnerability.XSS) vulnCount++;
                if (vulnerability.CSRF) vulnCount++;






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
                var query = string.Join("&", data.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                response = await _httpClient.GetAsync($"{action}?{query}");
            }

            return (await response.Content.ReadAsStringAsync(), (int)response.StatusCode);
        }
    }
}
