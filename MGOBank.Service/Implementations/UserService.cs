using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Domain.Roles;
using MGOBankApp.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.Service.Implementations
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
    }
}
