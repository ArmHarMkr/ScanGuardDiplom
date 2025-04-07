using ScanGuard.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScanGuard.BLL.Interfaces;
using ScanGuard.BLL.Services;
using ScanGuard.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace ScanGuard.Controllers
{
    [Authorize]
    public class CorporationController : Controller
    {
        private readonly ApplicationDbContext Context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> SignInManager;
        private readonly ICorpService _corpService;
        private readonly ILogger<CorpService> Logger;

        public CorporationController(ApplicationDbContext context, 
                                     UserManager<ApplicationUser> userManager, 
                                     SignInManager<ApplicationUser> signInamanger, 
                                     ICorpService corpService,
                                     ILogger<CorpService> _logger)
        {
            Context = context;
            _userManager = userManager;
            SignInManager = signInamanger;
            _corpService = corpService;
            Logger = _logger;
        }

        [HttpGet]
        public async Task<IActionResult> MyCorporation()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.Corporation == null)
            {
                TempData["ErrorMessage"] = "You don't have a corporation.";
            }
            var corporation = await Context.Corporations.FirstOrDefaultAsync(x => x.AdminUser == user);
            if(corporation == null)
            {
                TempData["ErrorMessage"] = "You don't have a corporation.";
            }
            return View(corporation);
        }

        [HttpGet]
        public async Task<IActionResult> AddCorp()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.Corporation != null)
            {
                TempData["ErrorMessage"] = "You already have a corporation.";
                return RedirectToAction("MyCorporation");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCorp(CorporationEntity corporation)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.Corporation != null)
            {
                TempData["ErrorMessage"] = "You already have a corporation.";
                return RedirectToAction("MyCorporation");
            }

            corporation.AdminUserId = user.Id;
            corporation.AdminUser = user;
            corporation.CreatedDate = DateTime.Now;
            await _corpService.CreteCorporation(corporation, user);
            return RedirectToAction("MyCorporation");
        }

        public async Task<IActionResult> RemoveCorporation(string corporationId)
        {
            var corporation = await _corpService.GetCorpEntity(corporationId);

            if (corporation == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(User.Identity.Name);

            if (user == null || user.Corporation?.Id != corporationId)
            {
                return Unauthorized();  // Handle unauthorized users
            }

            // Ensure the corporation entity is not null
            if (corporation.AdminUserId != user.Id)
            {
                return Unauthorized();  // Only the admin can remove the corporation
            }

            try
            {
                // Proceed with removing the corporation
                Context.Corporations.Remove(corporation);
                await Context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Handle any potential errors
                return BadRequest(ex.Message);
            }
        }


        public async Task<IActionResult> ChangeAdmin(string corporationId)
        {
            var corporation = await _corpService.GetCorpEntity(corporationId);

            if (corporation == null)
            {
                return NotFound();  // Return 404 if the corporation is not found
            }

            // Fetch the list of users in the corporation, ensure that it's not null
            var users = await Context.Users
                .Where(u => u.Corporation.Id == corporationId)
                .ToListAsync();

            // Guard against null users list
            if (users == null)
            {
                users = new List<ApplicationUser>();  // Assign an empty list if null
            }

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeCorpAdmin([FromBody] dynamic data)
        {
            string newAdminId = data.id;
            var currentUser = await _userManager.GetUserAsync(User);
            var newAdmin = await Context.Users.FirstOrDefaultAsync(u => u.Id == newAdminId);

            if (newAdmin == null)
            {
                return Json(new { message = "User not found." });
            }

            try
            {
                await _corpService.ChangeCorpAdminAsync(currentUser, newAdmin);
                return Json(new { message = "Admin changed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message });
            }
        }

        public async Task<IActionResult> GetAllCorporations()
        {
            var corporations = await _corpService.GetAllCorporations();
            return View(corporations);
        }

        public async Task<IActionResult> ViewRequest(string id)
        {
            var corp = await _corpService.GetCorpEntity(id);
            return View(corp);
        }


    }
}