using HtmlAgilityPack;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Models;
using MGOBankApp.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MGOBankApp.Controllers
{
    public class ScanController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IScannerService _scannerService;
        private readonly UserManager<ApplicationUser> UserManager;

        public ScanController(IHttpClientFactory httpClientFactory, IScannerService scannerService, UserManager<ApplicationUser> userManager)
        {
            _httpClient = httpClientFactory.CreateClient();
            _scannerService = scannerService;
            UserManager = userManager;
        }

        public IActionResult FwdScanner()
        {
            return RedirectToAction("Scanner");
        }

        [HttpGet]
        public IActionResult Scanner()
        {
            return View("~/Views/Scan/Scanner.cshtml", new Vulnerability());
        }

        [HttpPost]
        public async Task<IActionResult> Scanner(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                TempData["ErrorMessage"] = "URL не указан";
                return RedirectToAction("Scanner");
            }

            try
            {
                ApplicationUser? applicationUser = await UserManager.GetUserAsync(User);
                var result = await _scannerService.ScanUrl(url, applicationUser);

                TempData["SuccessMessage"] = "Сканирование завершено успешно.";
                return View("~/Views/Scan/Scanner.cshtml", result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
                return RedirectToAction("Scanner");
            }
        }
    }
}
