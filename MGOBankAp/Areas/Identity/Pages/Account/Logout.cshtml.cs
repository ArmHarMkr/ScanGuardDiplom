// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using MGOBankApp.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MGOBankApp.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();

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

            Log.Information("User {User} logged out. IP: {IP}", User.Identity?.Name ?? "Unknown", userIp);

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage();
            }
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
    }
}