using Microsoft.AspNetCore.Authorization;
using ScanGuard.BLL.Services;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using ScanGuard.Domain.Roles;
using ScanGuard.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ScanGuard.Service.Interfaces;

namespace ScanGuard.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext Context;
        private readonly IScannedSites ScannedSites;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly ILogger<OpenRouterService> _logger;

        public UserController(
            ApplicationDbContext context,
            IScannedSites scannedSites,
            UserManager<ApplicationUser> userManager,
            ILogger<OpenRouterService> logger)
        {
            Context = context;
            ScannedSites = scannedSites;
            UserManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> ShowScannedSites()
        {
            var currentUser = await UserManager.GetUserAsync(User);
            bool isPremium = await UserManager.IsInRoleAsync(currentUser, SD.Role_Premium);
            bool isAdmin = await UserManager.IsInRoleAsync(currentUser, SD.Role_Admin);

            var scannedSites = await Context.WebsiteScanEntities
                .Include(x => x.ScanUser)
                .Include(x => x.Vulnerabilities)
                .Where(x => x.ScanUser == currentUser)
                .ToListAsync();

            var resList = scannedSites.Select(scan => new SiteVulnViewModel
            {
                WebsiteScanId = scan.Id,
                ApplicationUser = scan.ScanUser,
                Url = scan.Url,
                ScanDate = scan.ScanDate,
                VulnerabilityCount = scan.VulnerablityCount,
                Vulnerabilities = scan.Vulnerabilities.ToList() ?? new List<VulnerabilityEntity>(),
                IsPremium = isPremium,
                IsAdmin = isAdmin
            }).ToList();

            return View(resList);
        }

        [HttpGet]
        public async Task<IActionResult> Analyze(string scanId)
        {
            var scan = await Context.WebsiteScanEntities
                .Include(x => x.Vulnerabilities)
                .FirstOrDefaultAsync(x => x.Id == scanId);

            if (scan == null) return NotFound();

            string scanResults = $"Scan for {scan.Url} found {scan.VulnerablityCount} vulnerabilities: " +
                                 string.Join(", ", scan.Vulnerabilities.Select(v => v.VulnerabilityType));

            OpenRouterService aiService = new OpenRouterService();
            string analysis = await aiService.GetAnalysisAsync(scanResults);

            return Json(new { analysis });
        }

        public async Task<IActionResult> RemoveSiteScan(string id)
        {
            var currentUser = await UserManager.GetUserAsync(User);
            var siteScan = await Context.WebsiteScanEntities
                .Include(x => x.ScanUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (siteScan == null || currentUser.Id != siteScan.ScanUser.Id)
            {
                return NotFound();
            }

            await ScannedSites.RemoveScannedSite(siteScan, currentUser);

            return RedirectToAction("ShowScannedSites");
        }
    }
}