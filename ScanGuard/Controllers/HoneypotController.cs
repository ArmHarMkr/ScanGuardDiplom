using Microsoft.AspNetCore.Mvc;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;

namespace ScanGuard.Controllers
{
    [Route("honeypot")]
    public class HoneypotController : Controller
    {
        private readonly ApplicationDbContext _context;  // Assuming you're using Entity Framework for DB
        private readonly ILogger<HoneypotController> _logger;

        public HoneypotController(ApplicationDbContext context, ILogger<HoneypotController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult LoginAttempt(string username, string password)
        {
            if (IsSqlInjection(username) || IsSqlInjection(password))
            {
                // Log the attacker's IP address and the attempt
                string attackerIp = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                _logger.LogWarning($"SQL Injection attempt from IP: {attackerIp} on honeypot login endpoint");

                // Store the attacker's IP in the DB for later analysis
                _context.HoneypotLogs.Add(new HoneypotLog
                {
                    IpAddress = attackerIp,
                    AttemptedUsername = username,
                    AttemptedPassword = password,
                    Timestamp = DateTime.Now
                });
                _context.SaveChanges();

                // Redirect to trolling page with the IP address
                return RedirectToAction("TrollPage", new { attackerIp });
            }

            return View();  // Handle the login normally if no SQLi is detected
        }


        [HttpGet]
        [Route("troll")]
        public IActionResult TrollPage(string attackerIp)
        {
            // Pass the attacker IP to the view
            ViewBag.AttackerIp = attackerIp;
            return View("TrollPage");  // Render the trolling message
        }

        private bool IsSqlInjection(string input)
        {
            string[] sqlPatterns = new string[]
            {
                "--", ";--", ";", "/*", "*/", "xp_", "exec", "select", "insert", "drop", "union", "information_schema"
            };

            return sqlPatterns.Any(pattern => input.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

    }
}
