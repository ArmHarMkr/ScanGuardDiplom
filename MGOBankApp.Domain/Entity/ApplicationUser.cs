using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MGOBankApp.Domain.Entity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public DateTime UserCreatedDate { get; set; } = DateTime.Now;
        public int ScannedUrlCount { get; set; } = 0;
        public int ScannedFileCount { get; set; } = 0;

        public virtual ICollection<WebsiteScanEntity> WebsiteScans { get; set; } // Если нужно
        public virtual ICollection<NotificationEntity> Notifications { get; set; } // Уведомления
        public virtual ICollection<FileScanEntity> FileScans { get; set; } // Сканирования файлов
    }
}
