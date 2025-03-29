using ScanGuard.Domain.Entity;

namespace ScanGuard.BLL.Interfaces
{
    public interface ICorpService
    {
        Task CreteCorporation(CorporationEntity corporation, ApplicationUser applicationUser);
        Task UpdateCorporation(CorporationEntity corporation, ApplicationUser applicationUser);
        Task RemoveCorporation(ApplicationUser applicationUser);
        Task<List<CorporationEntity>> GetAllCorporations ();
        CorporationEntity? GetUserCorporation(ApplicationUser appUser);
        Task<List<ApplicationUser>> GetCorpWorkersAsync(ApplicationUser adminUser);
        Task ChangeCorpAdminAsync(ApplicationUser currentAdmin, ApplicationUser newAdmin);
        Task<CorporationEntity> GetCorpEntity(string id);
    }
}
