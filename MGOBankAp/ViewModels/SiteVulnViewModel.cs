using MGOBankApp.Domain.Entity;
using MGOBankApp.Domain.Enums;

namespace MGOBankApp.ViewModels
{
    public class SiteVulnViewModel
    {
        public string WebsiteScanId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public string Url { get; set; }
        public DateTime ScanDate { get; set; }
        public int VulnerabilityCount { get; set; }
        public List<VulnerabilityEntity>? Vulnerabilities { get; set; }
        public bool IsPremium { get; set; }
        public bool IsAdmin{ get; set; }
    }
}
