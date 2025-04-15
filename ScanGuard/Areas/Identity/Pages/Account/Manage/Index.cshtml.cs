// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScanGuard.ViewModels;
using Microsoft.EntityFrameworkCore;
using ScanGuard.BLL.Interfaces;
using ScanGuard.Domain.Roles;

namespace ScanGuard.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _blobService;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IStorageService blobService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _blobService = blobService;
        }

        public IndexViewModel ProfileData { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var fullName = user.FullName ?? await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var isAdmin = await _userManager.IsInRoleAsync(user, SD.Role_Admin);
            var isPremium = await _userManager.IsInRoleAsync(user, SD.Role_Premium);

            ProfileData = new IndexViewModel
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                ProfilePhotoPath = user.ProfilePhotoPath,
                IsAdmin = isAdmin,
                IsPremium = isPremium
            };

            Input = new InputModel
            {
                FullName = fullName,
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Обновляем FullName
            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await LoadAsync(user);
                    return Page();
                }
            }

            // Обновляем номер телефона
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // Get user's IP
            string userIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Check if behind a proxy (Cloudflare, Nginx, etc.)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                userIp = Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }

            // Fetch public IPv4 if it's local or IPv6
            if (string.IsNullOrEmpty(userIp) || userIp == "::1" || userIp.StartsWith("10.") || userIp.StartsWith("192.168.") || userIp.StartsWith("172.16.") || userIp.Contains(":"))
            {
                userIp = await GetPublicIp();
            }

            if (user.RegistrationIpAddress == null)
            {
                user.RegistrationIpAddress = userIp;
            }
            await _context.SaveChangesAsync();
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePhotoAsync(IFormFile newProfilePhoto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (newProfilePhoto == null || newProfilePhoto.Length == 0)
            {
                StatusMessage = "Please select a valid image file.";
                return RedirectToPage();
            }

            // Валидация файла
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(newProfilePhoto.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                StatusMessage = "Invalid file type. Please upload a JPG, JPEG, PNG, or GIF image.";
                return RedirectToPage();
            }

            // Ограничение размера файла (например, 5MB)
            if (newProfilePhoto.Length > 5 * 1024 * 1024)
            {
                StatusMessage = "File size exceeds 5MB limit.";
                return RedirectToPage();
            }

            try
            {
                user.ProfilePhotoPath = await _blobService.UploadProfilePhotoAsync(newProfilePhoto, user.Id);
                await _userManager.UpdateAsync(user);
                StatusMessage = "Profile photo updated successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error uploading profile photo: " + ex.Message;
            }

            return RedirectToPage();
        }

        private static async Task<string> GetPublicIp()
        {
            using var httpClient = new HttpClient();
            try
            {
                return await httpClient.GetStringAsync("https://api64.ipify.org");
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}