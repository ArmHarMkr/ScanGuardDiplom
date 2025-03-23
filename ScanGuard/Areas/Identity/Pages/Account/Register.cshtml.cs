// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Domain.Roles;
using Serilog;

namespace MGOBankApp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>

            [Required(ErrorMessage = "Type you full name please")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        // Method to get the public IPv4 address
        private async Task<string> GetPublicIp()
        {
            try
            {
                using var client = new HttpClient();
                return await client.GetStringAsync("https://api4.ipify.org"); // Only IPv4
            }
            catch
            {
                return "Unable to retrieve public IPv4";
            }
        }


        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.FullName = Input.FullName;

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

                Log.Information("User {User} logged out. IP: {IP}", User.Identity?.Name ?? "Unknown", userIp);
                user.RegistrationIpAddress = userIp;
                user.LastLoginIpAddress = user.RegistrationIpAddress;
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {

                    Log.Information("User {UserName} registered successfully. IP : {IP}", user.Email, user.RegistrationIpAddress);
                    var userId = await _userManager.GetUserIdAsync(user);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var defaultRole = await _roleManager.FindByNameAsync(SD.Role_Customer);
                        if (defaultRole != null)
                        {
                            IdentityResult roleResult = await _userManager.AddToRoleAsync(user, defaultRole.Name);
                            if (!roleResult.Succeeded)
                            {
                                foreach (var error in roleResult.Errors)
                                {
                                    Log.Error(error.Description);
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                return Page(); // Return the page if role assignment fails
                            }
                        }

                        // Generate email confirmation token and URL
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        // Send confirmation email (optional, if email service is configured)
                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // Redirect to RegisterConfirmation page
                        return RedirectToPage("./RegisterConfirmation", new { email = Input.Email, emailConfirmationUrl = callbackUrl, returnUrl });
                    }
                    else
                    {
                        Log.Error("User ID is null or empty after creation.");
                        ModelState.AddModelError(string.Empty, "User creation failed unexpectedly.");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}