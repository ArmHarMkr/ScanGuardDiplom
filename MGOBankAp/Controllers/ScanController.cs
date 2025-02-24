using HtmlAgilityPack;
using MGOBankApp.Models;
using MGOBankApp.Service.Implementations;
using MGOBankApp.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MGOBankApp.Controllers
{
    public class ScanController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IScannerService _scannerService;

        public ScanController(IHttpClientFactory httpClientFactory,IScannerService scannerService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _scannerService = scannerService;
        }

        public IActionResult FwdScanner()
        {
            return RedirectToAction("Scanner");
        }

        [HttpGet]
        public IActionResult Scanner()
        {
            var vuln = new Vulnerability();
            return View(vuln);
        }

        [HttpPost]
        public async Task<IActionResult> Scanner(string url)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("URL не указан");

            try
            {
                var result = await _scannerService.ScanUrl(url);
                return View("Scanner",result);
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
