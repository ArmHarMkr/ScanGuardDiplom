using MGOBankApp.BLL.Services;
using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Domain.Roles;
using MGOBankApp.Service.Interfaces;
using MGOBankApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Drawing.Imaging;

namespace MGOBankApp.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly ApplicationDbContext Context;
    private readonly IScannedSites ScannedSites;
    private readonly UserManager<ApplicationUser> UserManager;
    private readonly ILogger<OpenRouterService> _logger;

    public UserController(ApplicationDbContext context, IScannedSites scannedSites, UserManager<ApplicationUser> userManager, ILogger<OpenRouterService> logger)
    {
        Context = context;
        ScannedSites = scannedSites;
        UserManager = userManager;
        _logger = logger;
    }


    public async Task<IActionResult> ShowScannedSites()
    {
        var currentUser = await UserManager.GetUserAsync(User);
        bool isPremium = await UserManager.IsInRoleAsync(currentUser, SD.Role_Premium); // Check role
        bool isAdmin= await UserManager.IsInRoleAsync(currentUser, SD.Role_Admin); // Check role

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

    [HttpGet]
    public async Task<IActionResult> UserProfile()
    {
        return View(await UserManager.GetUserAsync(User));
    }

    [HttpPost]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile profilePhoto)
    {
        if (profilePhoto != null && profilePhoto.Length > 0)
        {
            var user = await UserManager.GetUserAsync(User);
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

            var uniqueFileName = $"{user.Id}.jpg"; // User ID as filename
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            //changin size of photo
            //TODO: Refactor this code to a separate method
            using (var stream = new MemoryStream())
            {
                await profilePhoto.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                using (var originalImage = Image.FromStream(stream))
                {
                    int originalWidth = originalImage.Width;
                    int originalHeight = originalImage.Height;

                    int cropSize, finalSize;

                    if (originalWidth >= 1080 && originalHeight >= 1080)
                    {
                        // Если изображение больше 1080x1080, обрезаем и уменьшаем до 1080x1080
                        cropSize = Math.Min(originalWidth, originalHeight);
                        finalSize = 1080;
                    }
                    else
                    {
                        // Если изображение меньше 1080x1080, делаем квадрат по минимальной стороне
                        cropSize = Math.Min(originalWidth, originalHeight);
                        finalSize = cropSize; // Оставляем оригинальный размер
                    }

                    int x = (originalWidth - cropSize) / 2;
                    int y = (originalHeight - cropSize) / 2;

                    // Обрезаем изображение по центру
                    using (var croppedImage = new Bitmap(cropSize, cropSize))
                    {
                        using (var graphics = Graphics.FromImage(croppedImage))
                        {
                            graphics.DrawImage(originalImage, new Rectangle(0, 0, cropSize, cropSize),
                                               new Rectangle(x, y, cropSize, cropSize), GraphicsUnit.Pixel);
                        }

                        // Применяем нужный размер (либо 1080x1080, либо оригинальный)
                        using (var resizedImage = new Bitmap(croppedImage, new Size(finalSize, finalSize)))
                        {
                            resizedImage.Save(filePath, ImageFormat.Jpeg); // Сохраняем изображение
                        }
                    }
                }
            }


            user.ProfilePhotoPath = Path.Combine("wwwroot", "img", uniqueFileName).Replace("\\", "/");
            Context.Update(user);
            await Context.SaveChangesAsync();
        }

        return RedirectToAction("UserProfile");
    }

    [HttpPost]
    public async Task<IActionResult> ChangeProfilePhoto(IFormFile newProfilePhoto)
    {
        if (newProfilePhoto != null && newProfilePhoto.Length > 0)
        {
            var user = await UserManager.GetUserAsync(User);
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{user.Id}.jpg";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            if (!string.IsNullOrEmpty(user.ProfilePhotoPath))
            {
                var oldPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePhotoPath);
                if (System.IO.File.Exists(oldPhotoPath))
                {
                    System.IO.File.Delete(oldPhotoPath);
                }
            }
            using (var stream = new MemoryStream())
            {
                await newProfilePhoto.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                using (var originalImage = Image.FromStream(stream))
                {
                    int originalWidth = originalImage.Width;
                    int originalHeight = originalImage.Height;

                    int cropSize, finalSize;

                    if (originalWidth >= 1080 && originalHeight >= 1080)
                    {
                        cropSize = Math.Min(originalWidth, originalHeight);
                        finalSize = 1080;
                    }
                    else
                    {
                        cropSize = Math.Min(originalWidth, originalHeight);
                        finalSize = cropSize; 
                    }

                    int x = (originalWidth - cropSize) / 2;
                    int y = (originalHeight - cropSize) / 2;

                    using (var croppedImage = new Bitmap(cropSize, cropSize))
                    {
                        using (var graphics = Graphics.FromImage(croppedImage))
                        {
                            graphics.DrawImage(originalImage, new Rectangle(0, 0, cropSize, cropSize),
                                               new Rectangle(x, y, cropSize, cropSize), GraphicsUnit.Pixel);
                        }

                        using (var resizedImage = new Bitmap(croppedImage, new Size(finalSize, finalSize)))
                        {
                            resizedImage.Save(filePath, ImageFormat.Jpeg);
                        }
                    }
                }
            }


            user.ProfilePhotoPath = Path.Combine("wwwroot", "img", uniqueFileName).Replace("\\", "/");
            Context.Update(user);
            await Context.SaveChangesAsync();
        }

        return RedirectToAction("UserProfile");
    }

}
