using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.Domain.Entity
{
    public class TGUserEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ApplicationUser ApplicationUser { get; set; }
        public string TGUserToken { get; set; }
        public string? TGUserId { get; set; }
        public DateTime TGConnectedTime { get; set; } = DateTime.Now;
    }
}
