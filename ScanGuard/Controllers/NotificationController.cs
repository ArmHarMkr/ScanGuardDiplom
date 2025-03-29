using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;

namespace ScanGuard.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly ApplicationDbContext Context;

        public NotificationController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            UserManager = userManager;
            Context = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var currentUser = await UserManager.GetUserAsync(User);
            return View(await Context.NotificationEntities.Where(x => x.ApplicationUser == currentUser).ToListAsync());
        }
    }
}
