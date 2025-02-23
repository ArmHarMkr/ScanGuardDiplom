using HtmlAgilityPack;
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
        public ScannerService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<Vulnerability> ScanUrl(string url)
        {
            try
            {
                var baseUri = new Uri(url);
                _httpClient.BaseAddress = new Uri(baseUri.GetLeftPart(UriPartial.Authority));

                var response = await _httpClient.GetAsync(url);

                //Dont remove at this moment
                
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Ошибка доступа к сайту : {response.StatusCode}");

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var forms = doc.DocumentNode.SelectNodes("//form") ?? new HtmlNodeCollection(null);
                Vulnerability vulnerablity = new Vulnerability();

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

                    // Тест SQLi
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
                        vulnerablity.SQLi = true;
                        vulnCount++;
                    }

                    else if (sqliStatusCode >= 400 && normalStatusCode < 500)
                    {
                        vulnerablity.SQLi = true;
                        vulnCount++;
                    }

                    // Тест XSS
                    var xssPayload = "<script>alert('xss')</script>";
                    foreach (var key in data.Keys)
                        data[key] = xssPayload;

                    var (xssResponse, _) = await SendRequest(method, absoluteAction, data); // Код статуса не нужен
                    if (xssResponse.Contains(xssPayload))
                    {
                        vulnerablity.XSS = true;
                        vulnCount++;
                    }

                    // Тест XSRF
                    var csrfToken = form.SelectSingleNode(".//input[@name='csrf_token']");
                    if (csrfToken == null && method == "post")
                    {
                        vulnerablity.XSRF = true;
                        vulnCount++;
                    }
                }

               
                return vulnerablity;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сканирования: {ex.Message}");
            }
        }
        async Task<(string response, int statusCode)> SendRequest(string method, string action, Dictionary<string, string> data)
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
