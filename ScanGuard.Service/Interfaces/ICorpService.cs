using MGOBankApp.Domain.Entity;
using ScanGuard.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.BLL.Interfaces
{
    public interface ICorpService
    {
        Task CreteCorporation(CorporationEntity corporation);
        Task UpdateCorporation(CorporationEntity corporation, ApplicationUser applicationUser);
        Task RemoveCorporation(ApplicationUser applicationUser);
        Task<List<CorporationEntity>> GetAllCorporations ();
        CorporationEntity? GetUserCorporation(ApplicationUser appUser);
        Task<List<ApplicationUser>> GetCorpWorkersAsync(ApplicationUser adminUser);
        Task ChangeCorpAdminAsync(ApplicationUser currentAdmin, ApplicationUser newAdmin);
    }
}
