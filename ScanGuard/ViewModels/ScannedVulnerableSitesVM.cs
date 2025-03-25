using ScanGuard.Domain.Entity;

namespace ScanGuard.ViewModels
{
    public class ScannedVulnerableSitesVM
    {
        public List<WebsiteScanEntity> WebsiteScanEntities { get; set; }
        public bool IsSignedIn { get; set; }
        public List<VulnerabilityEntity> VulnerableSites { get; set;} = new List<VulnerabilityEntity>();
        
    }
}
