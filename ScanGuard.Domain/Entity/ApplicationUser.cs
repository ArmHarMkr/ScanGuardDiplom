using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ScanGuard.Domain.Entity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ScanGuard.Domain.Entity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public DateTime UserCreatedDate { get; set; } = DateTime.Now;
        public int ScannedUrlCount { get; set; } = 0;
        public int ScannedFileCount { get; set; } = 0;
        public bool TGConnected { get; set; } = false;
        public string ProfilePhotoPath { get; set; } = "wwwroot/img/default.jpg";
        public string? RegistrationIpAddress { get; set; }
        public string? LastLoginIpAddress { get; set; }
        public CorporationEntity? Corporation { get; set; }
        public virtual ICollection<WebsiteScanEntity> WebsiteScans { get; set; } 
        public virtual ICollection<NotificationEntity> Notifications { get; set; } 
        public virtual ICollection<FileScanEntity> FileScans { get; set; } 
    }
}
