using ScanGuard.BLL.Services;
using ScanGuard.DAL.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ScanGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScanApiController : ControllerBase
    {
        private readonly ApplicationDbContext Context; // Replace with your actual DbContext

        public ScanApiController(ApplicationDbContext context)
        {
            Context = context;
        }

        [HttpGet("analyze")] // Route: /api/ScanApi/analyze
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
            return Ok(new { analysis });
        }
    }
}
