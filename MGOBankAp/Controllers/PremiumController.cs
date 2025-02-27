using MGOBankApp.BLL.Interfaces;
using MGOBankApp.Domain.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MGOBankApp.Controllers
{
    [Authorize(Roles = SD.Role_Premium)]
    public class PremiumController : Controller
    {
        private readonly IFileScanService _scanService;

        public PremiumController(IFileScanService scanService)
        {
            _scanService = scanService;
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

            var result = await _scanService.ScanFileAsync(file, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return RedirectToAction("Result", new { id = result.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Result(string id)
        {
            var scanResult = await _scanService.GetScanResultAsync(id);
            if (scanResult == null)
                return NotFound();
            return View(scanResult);
        }
    }
}
