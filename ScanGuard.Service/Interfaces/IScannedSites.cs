using ScanGuard.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.Service.Interfaces
{
    public interface IScannedSites
    {
        Task<List<WebsiteScanEntity>> ShowAllScannedSites();
        Task<List<WebsiteScanEntity>> UserScannedSites(ApplicationUser applicationUser);
        Task<List<VulnerabilityEntity>> ShowSiteVulneraiblities(WebsiteScanEntity websiteScanEntity);

        Task RemoveScannedSite(WebsiteScanEntity websiteScanEntity, ApplicationUser applicationUser);
    }
}
