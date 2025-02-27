using MGOBankApp.BLL.Interfaces;
using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MGOBankApp.Controllers;

public class FileScanController : Controller
{
    private readonly IFileScanService _scanService;
    private const string RoleCustomer = "SD.Role_Customer";
    private const int DailyScanLimit = 2;
    private readonly ApplicationDbContext _context;

    public FileScanController(IFileScanService scanService, ApplicationDbContext applicationDbContext)
    {
        _scanService = scanService ?? throw new ArgumentNullException(nameof(scanService));
        _context = applicationDbContext;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Пожалуйста, выберите файл");
            return View("Index");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError("", "Пользователь не аутентифицирован");
            return View("Index");
        }

        // Проверяем роль пользователя в базе
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            ModelState.AddModelError("", "Пользователь не найден");
            return View("Index");
        }

        bool isCustomer = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == RoleCustomer); // Проверка роли

        if (isCustomer)
        {
            // Получаем последнюю проверку пользователя
            var lastScan = await _context.FileScanEntities
                .Where(x => x.ApplicationUserId == userId)
                .OrderByDescending(x => x.ScanDate)
                .FirstOrDefaultAsync();

            if (lastScan != null)
            {
                var timeSinceLastScan = DateTime.Now - lastScan.ScanDate;
                if (timeSinceLastScan.TotalHours < 24)
                {
                    TempData["Notification"] = "Вы можете проверять файлы 1 раз в 24 часа. Получите премиум для безлимитного доступа.";
                    return RedirectToAction("Index");
                }
                else
                {
                    try
                    {
                        var result = await _scanService.ScanFileAsync(file, userId);
                        return RedirectToAction("Result", new { id = result.Id });
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Ошибка при сканировании: {ex.Message}");
                        return View("Index");
                    }
                }
            }
            try
            {
                var result = await _scanService.ScanFileAsync(file, userId);
                return RedirectToAction("Result", new { id = result.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при сканировании: {ex.Message}");
                return View("Index");
            }
        }
        else
        {
            try
            {
                var result = await _scanService.ScanFileAsync(file, userId);
                return RedirectToAction("Result", new { id = result.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при сканировании: {ex.Message}");
                return View("Index");
            }

        }
    }

    [HttpGet]
    public async Task<IActionResult> Result(string id)
    {
        var scanResult = await _scanService.GetScanResultAsync(id);
        if (scanResult == null)
            return NotFound();
        return View(scanResult);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> History()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var history = await _scanService.GetUserScanHistoryAsync(userId);
        return View(history);
    }
}