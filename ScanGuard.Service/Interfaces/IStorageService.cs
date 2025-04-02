using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.BLL.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadProfilePhotoAsync(IFormFile profilePhoto, string userId);
        Task<string> ChangeProfilePhotoAsync(IFormFile newProfilePhoto, string userId);
        Task DeleteProfilePhotoAsync(string userId);
    }
}
