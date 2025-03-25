using HtmlAgilityPack;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using ScanGuard.Models;
using ScanGuard.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ScanGuard.Controllers;

public class ScanController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IScannerService _scannerService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public ScanController(
        IHttpClientFactory httpClientFactory,
        IScannerService scannerService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _httpClient = httpClientFactory.CreateClient();
        _scannerService = scannerService;
        _userManager = userManager;
        _context = context;
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
            ApplicationUser? applicationUser = await _userManager.GetUserAsync(User);
            var (vulnerability, portResults) = await _scannerService.ScanUrl(url, applicationUser);
            ViewBag.PortScanResults = portResults;

            if (applicationUser != null)
            {
                applicationUser.ScannedUrlCount++;
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Сканирование завершено успешно.";
            return View("~/Views/Scan/Scanner.cshtml", vulnerability);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
            return RedirectToAction("Scanner");
        }
    }
}
