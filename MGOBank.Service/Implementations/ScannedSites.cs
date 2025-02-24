using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.Service.Implementations
{
    public class ScannedSites : IScannedSites
    {
        private readonly ApplicationDbContext Context;

        public ScannedSites(ApplicationDbContext context)
        {
            Context = context;
        }

        public async Task RemoveScannedSite(WebsiteScanEntity websiteScanEntity, ApplicationUser applicationUser)
        {
            if (websiteScanEntity != null)
            {
                var vulnEntity = await Context.VulnerabilityEntities.Where(x => x.ScanEntity == websiteScanEntity).ToListAsync();
                Context.VulnerabilityEntities.RemoveRange(vulnEntity);
                Context.WebsiteScanEntities.Remove(websiteScanEntity);
                await Context.SaveChangesAsync();
            }

        }

        public async Task<List<WebsiteScanEntity>> ShowAllScannedSites()
        {
            return await Context.WebsiteScanEntities.Include(x => x.ScanUser).ToListAsync();
        }


        public async Task<List<VulnerabilityEntity>> ShowSiteVulneraiblities(WebsiteScanEntity websiteScanEntity)
        {
            return await Context.VulnerabilityEntities.Include(x => x.ScanEntity).ToListAsync();
        }

        public async Task<List<WebsiteScanEntity>> UserScannedSites(ApplicationUser applicationUser)
        {
            return await Context.WebsiteScanEntities.Include(x => x.ScanUser).Where(x => x.ScanUser == applicationUser).ToListAsync();
        }
    }
}
