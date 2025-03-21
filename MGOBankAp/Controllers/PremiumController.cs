using MGOBankApp.BLL.Interfaces;
using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Domain.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;

namespace MGOBankApp.Controllers
{
    [Authorize(Roles = $"{SD.Role_Premium}, {SD.Role_Admin}")]
    public class PremiumController : Controller
    {
        private readonly IFileScanService _scanService;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly ApplicationDbContext Context;
        private readonly INewsService _newsService;

        public PremiumController(IFileScanService scanService, UserManager<ApplicationUser> userManager, ApplicationDbContext context, INewsService newsService)
        {
            _scanService = scanService;
            UserManager = userManager;
            Context = context;
            _newsService = newsService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
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

        [HttpGet]
        public async Task<IActionResult> GetToken()
        {
            var currentUser = await UserManager.GetUserAsync(User);
            TGUserEntity? tgUserEntity = await Context.TGUserEntities.FirstOrDefaultAsync(x => x.ApplicationUser == currentUser);
            return View(tgUserEntity);
        }

        [HttpPost]
        public async Task<IActionResult> ShowToken()
        {
            var currentUser = await UserManager.GetUserAsync(User);
            if(!currentUser.TGConnected)
            {
                TGUserEntity tGUserEntity = new TGUserEntity();
                currentUser.TGConnected = true;
                tGUserEntity.TGUserToken = Guid.NewGuid().ToString();
                tGUserEntity.ApplicationUser = currentUser;
                Context.TGUserEntities.Add(tGUserEntity);
                await Context.SaveChangesAsync();

                return View("GetToken");
            }
            else
            {
                var tgUserEntity = await Context.TGUserEntities.FirstOrDefaultAsync(x => x.ApplicationUser.Id == currentUser.Id);
                TempData["Notification"] = $"You have already had your Token. {tgUserEntity.TGUserToken}";
                return View("Index");
            }

        }

        public async Task<IActionResult> GetAllNews()
        {
            return View(await _newsService.GetAllNews() ?? new List<NewsEntity>());
        }
    }
}
