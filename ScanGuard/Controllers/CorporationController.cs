using ScanGuard.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScanGuard.BLL.Interfaces;
using ScanGuard.BLL.Services;
using ScanGuard.Domain.Entity;
using ScanGuard.DAL.Data;

namespace ScanGuard.Controllers
{
    [Authorize]
    public class CorporationController : Controller
    {
        private readonly ApplicationDbContext Context;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly SignInManager<ApplicationUser> SignInManager;
        private readonly ICorpService CorpService;
        private readonly ILogger<CorpService> Logger;

        public CorporationController(ApplicationDbContext context, 
                                     UserManager<ApplicationUser> userManager, 
                                     SignInManager<ApplicationUser> signInamanger, 
                                     ICorpService corpService,
                                     ILogger<CorpService> _logger)
        {
            Context = context;
            UserManager = userManager;
            SignInManager = signInamanger;
            CorpService = corpService;
            Logger = _logger;
        }

        public async Task<IActionResult> MyCorporation()
        {
            var currentUser = await UserManager.GetUserAsync(User);
            var myCorp = currentUser!.Corporation ?? new CorporationEntity();
            return View(myCorp);
        }

        [HttpGet("AddCorp")]
        public async Task<IActionResult> AddCorp()
        {
            return View();
        }

        [HttpPost("AddCorp")]
        public async Task<IActionResult> AddCorp(CorporationEntity corporationEntity)
        {
            try
            {
                var currentUser = await UserManager.GetUserAsync(User);
                corporationEntity.AdminUser = await UserManager.GetUserAsync(User);
                await CorpService.CreteCorporation(corporationEntity, currentUser);
                await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error: {0}", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("AddCorp", new { id = corporationEntity.Id });
            }
            return RedirectToAction("MyCorporation", "Corporation");
        }

        [HttpGet("EditCorp")]
        public async Task<IActionResult> EditCorp()
        {
            var currentUser = await UserManager.GetUserAsync(User);
            var corporation = CorpService.GetUserCorporation(currentUser!);
            if (corporation == null) return NotFound();

            return View(corporation);
        }

        [HttpPost("EditCorp")]
        public async Task<IActionResult> EditCorp(CorporationEntity corporationEntity)
        {
            try
            {
                if (!ModelState.IsValid) return View(corporationEntity);
                var currentUser = await UserManager.GetUserAsync(User);
                await CorpService.UpdateCorporation(corporationEntity, currentUser!);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error: {0}", ex.Message);
                TempData["ErrorMessage"] = ex.Message; // Store exception in TempData
                return RedirectToAction("EditCorp", new { id = corporationEntity.Id }); // Redirect back to Edit page
            }

            return RedirectToAction("MyCorporation");
        }

        [HttpGet]
        public async Task<IActionResult> GetCorporationUsers()
        {
            var user = await UserManager.GetUserAsync(User);
            if (user?.Corporation == null || user.Corporation.AdminUser != user)
            {
                return Forbid();
            }

            var users = await CorpService.GetCorpWorkersAsync(user);
            return Json(users);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeCorpAdmin([FromBody] ApplicationUser newAdmin)
        {
            var currentUser = await UserManager.GetUserAsync(User);
            if (currentUser?.Corporation == null || currentUser.Corporation.AdminUser != currentUser)
            {
                return Forbid();
            }

            try
            {
                await CorpService.ChangeCorpAdminAsync(currentUser, newAdmin);
                return Ok(new { message = "Admin changed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("ChangeAdmin")]
        public async Task<IActionResult> ChangeAdmin()
        {
            var currentUser = await UserManager.GetUserAsync(User);
            if (currentUser?.Corporation == null || currentUser.Corporation.AdminUser != currentUser)
            {
                return Forbid();
            }

            var users = await CorpService.GetCorpWorkersAsync(currentUser);
            return View(users);
        }


    }
}