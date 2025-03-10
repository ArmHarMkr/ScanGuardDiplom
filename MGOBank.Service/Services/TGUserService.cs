using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Models;
using MGOBankApp.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace MGOBankApp.BLL.Services
{
    public class TGUserService

    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TGUserService> _logger;
        private string notConnectedUserErrorMessage = "You don't have access to use the ScanGuard telegram.\r\nGet premium, or if you already have it, connect your account to the bot <b>(/connect)</b>";
        public TGUserService(IServiceScopeFactory scopeFactory, IScannerService scannerService,ILogger<TGUserService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<string> ConnectUser(string token, string chatId)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (await IsVeryfiedUser(chatId) is not null)
            {
                return "You are already linked";
            }
            var user = await _context.TGUserEntities.Include(x => x.ApplicationUser).FirstOrDefaultAsync(x => x.TGUserToken == token);
            if (user == null)
            {
                return "Token is not valid";
            }
            else if (user.TGUserId is not null)
            {
                return "Token already linked";
            }

            user.TGUserId = chatId;
            user.TGConnectedTime = DateTime.Now;
            user.ApplicationUser.TGConnected = true;

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {Email} connected to telegram with telegramID {chatId}",user.ApplicationUser.Email,chatId);
            return "Connected";
        }
        public async Task<string> DisconnectUser(string chatId)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = await _context.TGUserEntities.Include(x => x.ApplicationUser).FirstOrDefaultAsync(x => x.TGUserId == chatId);
            if (user == null)
            {
                return "You don't have Telegram connected to ScanGuard";
            }


            user.TGUserId = null;
            user.ApplicationUser.TGConnected = false;
            user.TGConnectedTime = default;

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {Email} disconnected from telegram with telegramID {chatId}", user.ApplicationUser.Email, chatId);
            return "Disconnected";
        }
        public async Task<string> ScanUrl(string url, string chatId)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await _context.TGUserEntities.Include(x => x.ApplicationUser).FirstOrDefaultAsync(x => x.TGUserId == chatId);
            if (user == null)
            {
                return notConnectedUserErrorMessage;
            }
            var _scannerService = scope.ServiceProvider.GetRequiredService<IScannerService>();
            try
            {
                var resultVulnerability = await _scannerService.ScanUrl(url, user.ApplicationUser);

                return $@"
🔍 <b>Scan Results for:</b> {url}

🛡 <b>SQL Injection:</b> {(!resultVulnerability.vulnerability.SQLi ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>XSS:</b> {(!resultVulnerability.vulnerability.XSS ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>CSRF:</b> {(!resultVulnerability.vulnerability.CSRF ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>IDOR:</b> {(!resultVulnerability.vulnerability.IDOR ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>Broken Authentication:</b> {(!resultVulnerability.vulnerability.BrokenAuthentification ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>Security Misconfiguration:</b> {(!resultVulnerability.vulnerability.SecurityMisconfiguration ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>Unvalidated Redirect:</b> {(!resultVulnerability.vulnerability.UnvalidatedRedirectAndForwards ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>Directory Listing:</b> {(!resultVulnerability.vulnerability.DirectoryListing ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>LFI:</b> {(!resultVulnerability.vulnerability.LFI ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>RFI:</b> {(!resultVulnerability.vulnerability.RFI ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>HTTP Response Splitting:</b> {(!resultVulnerability.vulnerability.HTTPResponseSplitting ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>Phishing:</b> {(!resultVulnerability.vulnerability.Phishing ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>Not Secure Domain (HTTPS):</b> {(!resultVulnerability.vulnerability.HTTPWithoutS ? "✅ Secure" : "⚠️ Not Secure")}

📌 <b>Total:</b> <b>{(!resultVulnerability.vulnerability.SQLi &&
            !resultVulnerability.vulnerability.XSS &&
            !resultVulnerability.vulnerability.CSRF &&
            !resultVulnerability.vulnerability.IDOR &&
            !resultVulnerability.vulnerability.BrokenAuthentification &&
            !resultVulnerability.vulnerability.SecurityMisconfiguration &&
            !resultVulnerability.vulnerability.UnvalidatedRedirectAndForwards &&
            !resultVulnerability.vulnerability.DirectoryListing &&
            !resultVulnerability.vulnerability.LFI &&
            !resultVulnerability.vulnerability.RFI &&
            !resultVulnerability.vulnerability.HTTPResponseSplitting &&
            !resultVulnerability.vulnerability.Phishing &&
            !resultVulnerability.vulnerability.HTTPWithoutS ? "✅ Secure" : "⚠️ Vulnerable")}</b>

{(!resultVulnerability.vulnerability.SQLi &&
            !resultVulnerability.vulnerability.XSS &&
            !resultVulnerability.vulnerability.CSRF &&
            !resultVulnerability.vulnerability.IDOR &&
            !resultVulnerability.vulnerability.BrokenAuthentification &&
            !resultVulnerability.vulnerability.SecurityMisconfiguration &&
            !resultVulnerability.vulnerability.UnvalidatedRedirectAndForwards &&
            !resultVulnerability.vulnerability.DirectoryListing &&
            !resultVulnerability.vulnerability.LFI &&
            !resultVulnerability.vulnerability.RFI &&
            !resultVulnerability.vulnerability.HTTPResponseSplitting &&
            !resultVulnerability.vulnerability.Phishing &&
            !resultVulnerability.vulnerability.HTTPWithoutS
            ? "🎉 Your website is well-protected! No vulnerabilities found."
            : "⚠️ Security Alert! Your website has vulnerabilities that need fixing.")}";
            }
            catch (Exception)
            {
                return "Invalid link";
            }
        }



        public async Task<(string profileInfo, string profileImageUrl)> GetProfileInfo(string chatId)
        {
            var user = await IsVeryfiedUser(chatId);
            if (user == null)
            {
                return (notConnectedUserErrorMessage, null!);
            }
            var profileImageUrl = user.ApplicationUser.ProfilePhotoPath;
            var profileInfo = $@"
<b>👤 Name:</b> {user.ApplicationUser.FullName}
<b>📧 Email:</b> {user.ApplicationUser.Email}
<b>📅 Registered:</b> {user.ApplicationUser.UserCreatedDate:dd.MM.yyyy}

<b>📊 Scanned Websites:</b> {user.ApplicationUser.ScannedUrlCount}
<b>📂 Scanned Files:</b> {user.ApplicationUser.ScannedFileCount}
";
            return (profileInfo, profileImageUrl);
        }
        private async Task<TGUserEntity?> IsVeryfiedUser(string chatId)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await _context.TGUserEntities.Include(x => x.ApplicationUser).FirstOrDefaultAsync(x => x.TGUserId == chatId);
        }
    }
}