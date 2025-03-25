using ScanGuard.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.Domain.Entity
{
    public class CorporationEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CorpName { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string? Zip { get; set; }
        public string Country { get; set; }
        public int WorkerCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public ApplicationUser? AdminUser { get; set; }
        public bool IsSubmitted { get; set; } = false;
    }
}
