using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScanGuard.Domain.Enums;

namespace ScanGuard.Domain.Entity
{
    public class FileScanEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; }
        public string FileHash { get; set; }
        public DateTime ScanDate { get; set; } = DateTime.Now;
        public string VirusTotalReport { get; set; }
        public bool IsMalicious { get; set; }
        public VulnerabilityType VulnerabilityType { get; set; }

        // Связь с пользователем (опционально)
        public string? ApplicationUserId { get; set; } // Идентификатор пользователя
        public virtual ApplicationUser? ScannedByUser { get; set; } // Навигационное свойство
    }
}
