using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.Domain.Entity
{
    public class CreateCorpRequestEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public CorporationEntity Corporation { get; set; }
        public DateTime RequestedTime { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false;
    }
}
