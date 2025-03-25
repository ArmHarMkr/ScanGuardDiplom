using ScanGuard.BLL.Interfaces;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using ScanGuard.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        public AdminController(ApplicationDbContext context,
                               SignInManager<ApplicationUser> signInManager,
                               UserManager<ApplicationUser> usermanager,
                               IUserService userservice,
                               INewsService newsService)
        {
            Context = context;
            SignInManager = signInManager;
            UserManager = usermanager;
            UserService = userservice;
            _newsService = newsService;
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
    }
}
