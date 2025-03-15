// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MGOBankApp.Domain.Entity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Serilog;
using MGOBankApp.BLL.Interfaces;
using Telegram.Bot.Types;

namespace MGOBankApp.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger,IEmailService emailService)
        {
            _signInManager = signInManager;
            _emailService = emailService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Get user's IP
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ip = Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }

            // Check if the IP is private and fetch public IP
            if (ip.StartsWith("10.") || ip.StartsWith("192.168.") || ip.StartsWith("172.16.") || ip == "::1")
            {
                ip = await GetPublicIp();
            }

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);

                    if (user == null)
                    {
                        Log.Warning("Login attempt failed: User not found. Email: {Email}", Input.Email);
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }

                    Log.Information("User {UserName} logged in. IP: {IP}", Input.Email, ip);

                    // Check if IP is different from stored IP
                    if (string.IsNullOrEmpty(user.RegistrationIpAddress) || user.RegistrationIpAddress != ip)
                    {
                        await _emailService.SendSecurityAlertEmail(user.Email, user.UserName, user.RegistrationIpAddress, ip);
                        Log.Warning("User {UserName} IP address changed. Old IP: {OldIp}, New IP: {NewIp}", Input.Email, user.RegistrationIpAddress, ip);

                        // Update user's stored IP to the new one
                        user.LastLoginIpAddress = ip;
                        await _signInManager.UserManager.UpdateAsync(user);
                    }

                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    Log.Warning("User {UserName} account locked out. IP: {IP}", Input.Email, ip);
                    return RedirectToPage("./Lockout");
                }

                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "You are not allowed.");
                    return Page();
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            return Page();
        }

        private async Task<string> GetPublicIp()
        {
            try
            {
                using var client = new HttpClient();
                return await client.GetStringAsync("https://api4.ipify.org"); // Fetches IPv4 only
            }
            catch
            {
                return "Unable to retrieve public IPv4";
            }
        }

    }
}
