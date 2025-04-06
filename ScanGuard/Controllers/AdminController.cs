using ScanGuard.BLL.Interfaces;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using ScanGuard.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ScanGuard.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext Context;
        private readonly SignInManager<ApplicationUser> SignInManager;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly IUserService UserService;
        private readonly INewsService _newsService;
        private readonly ICorpService CorporationService;

        public AdminController(ApplicationDbContext context,
                               SignInManager<ApplicationUser> signInManager,
                               UserManager<ApplicationUser> usermanager,
                               IUserService userservice,
                               INewsService newsService,
                               ICorpService corpService)
        {
            Context = context;
            SignInManager = signInManager;
            UserManager = usermanager;
            UserService = userservice;
            _newsService = newsService;
            CorporationService = corpService;
        }



        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await UserManager.GetUserAsync(User);
                var allUsers = await UserService.GetAllUsers();
                return View(allUsers.Where(x => x.Id != currentUser.Id).ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GivePremiumRole")]
        public async Task<IActionResult> GivePremiumRole(string? id)
        {
            if (id == null) return NotFound();
            var userFromDb = await UserService.GetApplicationUser(id);
            await UserService.GivePremiumRole(userFromDb);
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost("GiveCustomerRole")]
        public async Task<IActionResult> GiveCustomerRole(string? id)
        {
            if (id == null) return NotFound();
            var userFromDb = await UserService.GetApplicationUser(id);
            await UserService.GiveCustomerRole(userFromDb);
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost("GiveAdminRole")]
        public async Task<IActionResult> GiveAdminRole(string? id)
        {
            if (id == null) return NotFound();
            var userFromDb = await UserService.GetApplicationUser(id);
            await UserService.GiveAdminRole(userFromDb);
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet("AddNews")]
        public async Task<IActionResult> AddNews()
        {
            return View();
        }

        [HttpPost("AddNews")]   
        public async Task<IActionResult> AddNews(NewsEntity newsEntity)
        {
            await _newsService.CreteNews(newsEntity);
            return RedirectToAction("AllNews", "Admin");
        }


        public async Task<IActionResult> AllNews()
        {
            return View(await _newsService.GetAllNews() ?? new List<NewsEntity>());
        }


        [HttpPost("RemoveNews")]
        public async Task<IActionResult> RemoveNews(string? id)
        {
            if (id == null) return NotFound();
            await _newsService.RemoveNews(id);
            return RedirectToAction("AllNews", "Admin");
        }

        public async Task<IActionResult> GetAllCorporations()
        {
            var requests = await Context.CreateCorpRequests
                .Include(x => x.Corporation)
                .Where(x => !x.IsApproved)
                .ToListAsync();
            return View(requests);
        }

        public async Task<IActionResult> AcceptCorporation(string id)
        {
            try
            {
                var request = await Context.CreateCorpRequests.FirstOrDefaultAsync(x => x.Id == id);
                await UserService.ApproveCorp(request!);
                NotificationEntity notification = new NotificationEntity
                {
                    NotificationTitle = "Your corporation has been approved",
                    NotificationContent = "Your corporation has been approved by the admin",
                    ApplicationUser = request!.Corporation.AdminUser!
                };
                await Context.NotificationEntities.AddAsync(notification);
                await Context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("GetAllCorporations");
        }
        public async Task<IActionResult> DisapproveCorporation(string id)
        {
            try
            {
                var request = await Context.CreateCorpRequests.FirstOrDefaultAsync(x => x.Id == id);
                await UserService.DisapproveCorp(request!);
                NotificationEntity notification = new NotificationEntity
                {
                    NotificationTitle = "Your corporation has been disapproved",
                    NotificationContent = "Your corporation has been disapproved by the admin",
                    ApplicationUser = request!.Corporation.AdminUser!
                };
                await Context.NotificationEntities.AddAsync(notification);
                await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("GetAllCorporations");
        }

        public async Task<IActionResult> ViewRequest(string id)
        {
            var request = await Context.CreateCorpRequests.Include(x => x.Corporation).ThenInclude(x => x.AdminUser).FirstOrDefaultAsync(x => x.Id == id);
            if (request == null) return NotFound();
            return View("~/Views/Admin/ViewRequest.cshtml", request);
        }
    }
}
