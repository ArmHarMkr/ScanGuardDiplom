using HtmlAgilityPack;
using MGOBankApp.BLL.Interfaces;
using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Models;
using MGOBankApp.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MGOBankApp.Service.Implementations
{
    public class ScannerService : IScannerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVulnerabilityAnalyzer _analyzer;
        private readonly IServiceScopeFactory _scopeFactory;

        public ScannerService(IHttpClientFactory httpClientFactory, IVulnerabilityAnalyzer analyzer, UserManager<ApplicationUser> userManager, IServiceScopeFactory scopeFactory)
        {
            _userManager = userManager;
            _analyzer = analyzer;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        public async Task<Vulnerability> ScanUrl(string url, ApplicationUser? applicationUser)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();

                Vulnerability vulnerability = new Vulnerability();
                var baseUri = new Uri(url);
                httpClient.BaseAddress = new Uri(baseUri.GetLeftPart(UriPartial.Authority));

                // Создаем скоуп для контекста
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Проверяем, есть ли такой сайт в базе данных, если да, увеличиваем счетчик
                    var siteScanCount = await _context.SiteScanCounts.FirstOrDefaultAsync(x => x.Url == httpClient.BaseAddress.ToString());
                    if (siteScanCount != null)
                    {
                        siteScanCount.CheckCount++;
                    }
                    else
                    {
                        // Создаем новый объект для сайта, если его еще нет в базе
                        siteScanCount = new SiteScanCountEntity()
                        {
                            Url = httpClient.BaseAddress.ToString(),
                            CheckCount = 1
                        };
                        _context.SiteScanCounts.Add(siteScanCount);
                    }

                    // Сохраняем изменения в базе данных
                    await _context.SaveChangesAsync();

                    // Проверяем, доступен ли сайт
                    var response = await httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Ошибка доступа к сайту : {response.StatusCode}");

                    var html = await response.Content.ReadAsStringAsync();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var forms = doc.DocumentNode.SelectNodes("//form") ?? new HtmlNodeCollection(null);

                    vulnerability.HTTPWithoutS = _analyzer.IsHttps(baseUri);

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
                        // Прежде чем добавлять запись в таблицу WebsiteScanEntities, проверяем уникальность
                        var existingScan = await _context.WebsiteScanEntities
                            .Where(x => x.Url == url && x.ScanUser.Id == applicationUser.Id)
                            .FirstOrDefaultAsync();

                        if (existingScan != null)
                        {
                            // Если такой скан уже есть, можно обновить или проигнорировать
                            existingScan.VulnerablityCount = vulnCount;
                            _context.WebsiteScanEntities.Update(existingScan);
                        }
                        else
                        {
                            // Если скан еще не существует, добавляем новый
                            WebsiteScanEntity websiteScanEntity = new WebsiteScanEntity()
                            {
                                ScanUser = applicationUser,
                                Status = "Scanned",
                                Url = url,
                                VulnerablityCount = vulnCount
                            };

                            _context.WebsiteScanEntities.Add(websiteScanEntity);

                            // Добавляем уязвимости в зависимости от результатов тестов
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
                        }

                        // Сохраняем изменения в базе данных
                        await _context.SaveChangesAsync();
                    }
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
