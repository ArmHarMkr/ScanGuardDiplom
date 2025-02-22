using HtmlAgilityPack;
using MGOBankApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace MGOBankApp.Controllers
{
    public class ScanController : Controller
    {
        private readonly HttpClient _httpClient;

        public ScanController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost]
        public IActionResult Scanner()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ScanUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("URL не указан");

            try
            {
                // Получаем страницу
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, "Ошибка доступа к сайту");

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Находим формы
                var forms = doc.DocumentNode.SelectNodes("//form") ?? new HtmlNodeCollection(null);
                Vulnerability vulnerablity = new Vulnerability();

                foreach (var form in forms)
                {
                    var action = form.GetAttributeValue("action", url);
                    var method = form.GetAttributeValue("method", "get").ToLower();
                    var inputs = form.SelectNodes(".//input");

                    // Собираем данные формы
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

                    // Тест SQLi
                    var sqliPayload = "' OR 1=1 --";
                    foreach (var key in data.Keys)
                        data[key] = sqliPayload;

                    var sqliResponse = await SendRequest(method, action, data);
                    if (sqliResponse.ToLower().Contains("mysql") || sqliResponse.ToLower().Contains("sql"))
                    {
                        vulnerablity.SQLi = true;
                    }

                    // Тест XSS
                    var xssPayload = "<script>alert('xss')</script>";
                    foreach (var key in data.Keys)
                        data[key] = xssPayload;

                    var xssResponse = await SendRequest(method, action, data);
                    if (xssResponse.Contains(xssPayload))
                    {
                        vulnerablity.XSS = true;
                    }

                    // Тест XSRF (проверка наличия токена)
                    var csrfToken = form.SelectSingleNode(".//input[@name='csrf_token']");
                    if (csrfToken == null && method == "post")
                    {
                        vulnerablity.XSRF = true;
                    }
                }

                return View(vulnerablity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        private async Task<string> SendRequest(string method, string action, Dictionary<string, string> data)
        {
            if (method == "post")
            {
                var content = new FormUrlEncodedContent(data);
                var response = await _httpClient.PostAsync(action, content);
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                var query = string.Join("&", data.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                var response = await _httpClient.GetAsync($"{action}?{query}");
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
