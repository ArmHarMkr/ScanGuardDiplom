using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Service.Interfaces;
using MGOBankApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace MGOBankApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext Context;
        private readonly IScannedSites ScannedSites;
        private readonly UserManager<ApplicationUser> UserManager;

        public UserController(ApplicationDbContext context, IScannedSites scannedSites, UserManager<ApplicationUser> userManager)
        {
            Context = context;
            ScannedSites = scannedSites;
            UserManager = userManager;
        }


        public async Task<IActionResult> ShowScannedSites()
        {
            var currentUser = await UserManager.GetUserAsync(User);
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
                Vulnerabilities = scan.Vulnerabilities.ToList() ?? new List<VulnerabilityEntity>()
            }).ToList();

            return View(resList);
        }

        public async Task<IActionResult> RemoveSiteScan(string id)
        {
            var currentUser = await UserManager.GetUserAsync(User);
            var siteScan = await Context.WebsiteScanEntities.Include(x => x.ScanUser).FirstOrDefaultAsync(x => x.Id == id);
            if (siteScan == null || currentUser.Id != siteScan.ScanUser.Id)
            {
                return NotFound();
            }

            await ScannedSites.RemoveScannedSite(siteScan, currentUser);

            // Fetch the updated list of scanned sites and pass it to the view
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
                Vulnerabilities = scan.Vulnerabilities.ToList() ?? new List<VulnerabilityEntity>()
            }).ToList();

            return View("ShowScannedSites", resList);
        }

    }
}
