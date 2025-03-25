using ScanGuard.Domain.Entity;
using ScanGuard.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ScanGuard.Service.Interfaces
{
    public interface IUserService
    {
        Task GivePremiumRole(ApplicationUser applicationUser);
        Task GiveCustomerRole(ApplicationUser applicationUser);
        Task GiveAdminRole(ApplicationUser applicationUser);
        
        Task<List<ApplicationUser>> GetAllUsers();
        Task<ApplicationUser> GetApplicationUser(string id);
        Task AddToCorp(CorporationEntity corporationEntity);
        Task RemoveFromCorp(ApplicationUser appUser, CorporationEntity corporationEntity);
    }
}
