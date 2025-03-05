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
        private string notConnectedUserErrorMessage = "You don't have access to use the ScanGuard telegram.\r\nGet premium, or if you already have it, connect your account to the bot <b>(/connect)</b>";
        private readonly ILogger<TGUserService> _logger;
        public TGUserService(IServiceScopeFactory scopeFactory, IScannerService scannerService,ILogger<TGUserService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<string> ConnectUser(string token, string chatId)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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

🛡 <b>XSS Protection:</b> {(!resultVulnerability.XSS ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>SQL Injection Protection:</b> {(!resultVulnerability.SQLi ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>CSRF Protection:</b> {(!resultVulnerability.CSRF ? "✅ Secure" : "⚠️ Vulnerable")}
🛡 <b>HTTPS Enabled:</b> {(!resultVulnerability.HTTPWithoutS ? "✅ Yes" : "⚠️ No")}

📌 <b>Total:</b> <b>{(!resultVulnerability.XSS && !resultVulnerability.SQLi && !resultVulnerability.CSRF && !resultVulnerability.HTTPWithoutS ? "✅ Secure" : "⚠️ Vulnerable")}</b>

{(!resultVulnerability.XSS && !resultVulnerability.SQLi && !resultVulnerability.CSRF && !resultVulnerability.HTTPWithoutS
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