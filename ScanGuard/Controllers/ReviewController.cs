using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using ScanGuard.ViewModels;

namespace ScanGuard.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var hasReview = _context.ReviewEntities.Any(r => r.User.Id == user.Id);

            var model = new ReviewViewModel
            {
                CanAdd = !hasReview,
                Reviews = _context.ReviewEntities.Include(r => r.User).OrderByDescending(r => r.DateTime).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(string reviewText, bool IsGood)
        {
            var user = await _userManager.GetUserAsync(User);
            var alreadyExists = _context.ReviewEntities.Any(r => r.User.Id == user.Id);
            if (alreadyExists)
                return RedirectToAction("Index");

            var review = new ReviewEntity
            {
                ReviewText = reviewText,
                IsGood = IsGood,
                User = user!
            };

            _context.ReviewEntities.Add(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var review = await _context.ReviewEntities.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            var user = await _userManager.GetUserAsync(User);

            if (review != null && review.User.Id == user.Id)
            {
                _context.ReviewEntities.Remove(review);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }



    }
}
