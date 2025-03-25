using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using ScanGuard.BLL.Interfaces;
using ScanGuard.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.BLL.Services
{
    public class CorpService : ICorpService
    {
        private readonly ApplicationDbContext Context;
        public CorpService(ApplicationDbContext context)
        {
            Context = context;
        }

        public async Task<List<ApplicationUser>> GetCorpWorkersAsync(ApplicationUser adminUser)
        {
            if (adminUser?.Corporation == null)
            {
                throw new Exception("Admin is not part of any corporation");
            }
            return await Context.Users.Where(u => u.Corporation == adminUser.Corporation).ToListAsync();
        }

        public async Task ChangeCorpAdminAsync(ApplicationUser currentAdmin, ApplicationUser newAdmin)
        {
            if (currentAdmin?.Corporation == null || currentAdmin.Corporation.AdminUser != currentAdmin)
            {
                throw new Exception("Only the current admin can change the corporation admin");
            }

            if (newAdmin == null || newAdmin.Corporation != currentAdmin.Corporation)
            {
                throw new Exception("Selected user is not part of the corporation");
            }

            currentAdmin.Corporation.AdminUser = newAdmin;
            Context.Corporations.Update(currentAdmin.Corporation);
            await Context.SaveChangesAsync();
        }

        public async Task CreteCorporation(CorporationEntity corporation)
        {
            Context.Corporations.Add(corporation);
            CreateCorpRequestEntity request = new CreateCorpRequestEntity
            {
                Corporation = corporation
            };
            Context.CreateCorpRequests.Add(request);
            await Context.SaveChangesAsync();
        }

        public async Task<List<CorporationEntity>> GetAllCorporations()
        {
            var list = await Context.Corporations.ToListAsync() ?? new List<CorporationEntity>();
            return list;
        }

        public CorporationEntity GetUserCorporation(ApplicationUser appUser)
        {
            return appUser.Corporation!;
        }

        public async Task RemoveCorporation(ApplicationUser applicationUser)
        {
            var corp = applicationUser.Corporation;
            if (corp != null)
            {
                if (corp.AdminUser == applicationUser)
                {
                    Context.Corporations.Remove(corp);
                    await Context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("You are not allowed to remove this corporation!");
                }
            }
            else
            {
                throw new Exception("You don't have a corporation to remove!");
            }
        }

        public async Task UpdateCorporation(CorporationEntity corporation, ApplicationUser applicationUser)
        {
            if (corporation != null)
            {
                if (corporation.AdminUser == applicationUser)
                {
                    Context.Corporations.Update(corporation);
                    await Context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("You are not allowed to update this corporation!");
                }
            }
        }
    }
}
