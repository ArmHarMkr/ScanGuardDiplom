using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MGOBankApp.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _context.ChatMessages.OrderBy(m => m.DateTime).ToListAsync();
            var user = await _userManager.GetUserAsync(User);

            ViewData["UserFullName"] = user?.FullName ?? "";
            ViewData["UserProfilePhoto"] = user?.ProfilePhotoPath ?? "wwwroot/img/default.jpg";
            return View(messages);
        }


        [HttpGet]
        public async Task<JsonResult> GetMessages()
        {
            var messages = await _context.ChatMessages
                .OrderBy(m => m.DateTime)
                .ToListAsync();
            return Json(messages);
        }

    }

}
