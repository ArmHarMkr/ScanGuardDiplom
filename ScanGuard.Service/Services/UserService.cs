using HtmlAgilityPack;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using ScanGuard.Domain.Roles;
using ScanGuard.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ScanGuard.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.Service.Implementations
{
    public class UserService : IUserService
    {
        private ApplicationDbContext Context;
        private readonly UserManager<ApplicationUser> UserManager;
        public UserService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            Context = db;
            UserManager = userManager;
        }



        public async Task GiveAdminRole(ApplicationUser applicationUser)
        {
            await UserManager.RemoveFromRoleAsync(applicationUser, SD.Role_Customer);
            await UserManager.RemoveFromRoleAsync(applicationUser, SD.Role_Premium);
            await UserManager.AddToRoleAsync(applicationUser, SD.Role_Admin);
        }

        public async Task GiveCustomerRole(ApplicationUser applicationUser)
        {
            await UserManager.RemoveFromRoleAsync(applicationUser, SD.Role_Admin);
            await UserManager.RemoveFromRoleAsync(applicationUser, SD.Role_Premium);
            await UserManager.AddToRoleAsync(applicationUser, SD.Role_Customer);
        }

        public async Task GivePremiumRole(ApplicationUser applicationUser)
        {
            await UserManager.RemoveFromRoleAsync(applicationUser, SD.Role_Customer);
            await UserManager.RemoveFromRoleAsync(applicationUser, SD.Role_Admin);
            await UserManager.AddToRoleAsync(applicationUser, SD.Role_Premium);
        }

        public async Task<List<ApplicationUser>> GetAllUsers()
        {
            var allUsers = await Context.Users.ToListAsync();
            return allUsers;
        }

        public async Task<ApplicationUser> GetApplicationUser(string id)
        {
            if (id != null || !id.IsNullOrEmpty())
            {
                return await Context.Users.FirstOrDefaultAsync(x => x.Id == id);
            }
            else
            {
                throw new Exception("No User Found");
            }
        }

        public async Task AddToCorp(CorporationEntity corporationEntity)
        {
            await Context.Corporations.AddAsync(corporationEntity);
            await Context.SaveChangesAsync();
        }

        public async Task RemoveFromCorp(ApplicationUser appUser, CorporationEntity corporationEntity)
        {
            if(appUser.Corporation == corporationEntity)
            {
                Context.Corporations.Remove(corporationEntity);
                await Context.SaveChangesAsync();
            }
        }

        public async Task ApproveCorp(CreateCorpRequestEntity request)
        {
            request.IsApproved = true;
            request.Corporation.IsSubmitted = true;
            await Context.SaveChangesAsync();
        }
        public async Task DisapproveCorp(CreateCorpRequestEntity request)
        {
            Context.Corporations.Remove(request.Corporation);
            Context.CreateCorpRequests.Remove(request);
            await Context.SaveChangesAsync();
        }

    }
}
