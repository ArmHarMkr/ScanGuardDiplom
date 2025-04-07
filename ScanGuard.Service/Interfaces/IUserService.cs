using ScanGuard.Domain.Entity;


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

        Task ApproveCorp(CreateCorpRequestEntity request);
        Task DisapproveCorp(CreateCorpRequestEntity request);
    }
}
