using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Models;
using MGOBankApp.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using static System.Net.WebRequestMethods;

namespace MGOBankApp.BLL.Services
{
    public class TGUserService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IScannerService _scannerService;

        public TGUserService(IServiceScopeFactory scopeFactory, IScannerService scannerService)
        {
            _scopeFactory = scopeFactory;
            _scannerService = scannerService;
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
            user.TGConnectedTime = default;
            user.ApplicationUser.TGConnected = false;
            await _context.SaveChangesAsync();
            return "Disconnected";
        }
        public async Task<string> ScanUrl(string chatId, string url)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await _context.TGUserEntities.Include(x => x.ApplicationUser).FirstOrDefaultAsync(x => x.TGUserId == chatId);
            if (user == null)
            {
                return "You don't have access to use the ScanGuard telegram.\r\nGet premium, or if you already have it, connect your account to the bot <b>(/connect)</b>";
            }
            var resultVulnerability = await _scannerService.ScanUrl(url, user.ApplicationUser);

            return $@"
                🔍 <b>Scan Results for:</b> {url}

🛡️ <b>XSS Protection:</b> {(!resultVulnerability.XSS ? "✅ Secure" : "⚠️ Vulnerable")}
🛡️ <b>SQL Injection Protection:</b> {(!resultVulnerability.SQLi ? "✅ Secure" : "⚠️ Vulnerable")}
🛡️ <b>CSRF Protection:</b> {(!resultVulnerability.CSRF ? "✅ Secure" : "⚠️ Vulnerable")}
🛡️ <b>HTTPS Enabled:</b> {(resultVulnerability.HTTPWithoutS ? "✅ Yes": "⚠️ No")}

📌 <b>Total:</b> <b>{(resultVulnerability.XSS && resultVulnerability.SQLi && resultVulnerability.CSRF && !resultVulnerability.HTTPWithoutS ? "✅ Secure" : "⚠️ Vulnerable")}</b>

 {(resultVulnerability.XSS && resultVulnerability.SQLi && resultVulnerability.CSRF && !resultVulnerability.HTTPWithoutS
? "🎉 Your website is well-protected! No vulnerabilities found."
: "⚠️ Security Alert! Your website has vulnerabilities that need fixing.")}"; ;
        }
    }
}