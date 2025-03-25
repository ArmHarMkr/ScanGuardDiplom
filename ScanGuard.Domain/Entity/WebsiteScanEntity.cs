using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.Domain.Entity
{
    public class WebsiteScanEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ApplicationUser? ScanUser { get; set; }
        public string Url { get; set; }
        public DateTime ScanDate { get; set; } = DateTime.Now;
        public string Status { get; set; }
        public int VulnerablityCount { get; set; }

        public virtual ICollection<VulnerabilityEntity>? Vulnerabilities { get; set; }
    }
}
