using MGOBankApp.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MGOBankApp.Service.Interfaces
{
    public interface IUserService
    {
        Task GivePremiumRole(ApplicationUser applicationUser);
        Task GiveCustomerRole(ApplicationUser applicationUser);
        Task GiveAdminRole(ApplicationUser applicationUser);
        
        Task<List<ApplicationUser>> GetAllUsers();
        Task<ApplicationUser> GetApplicationUser(string id);
    }
}
