using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.Domain.Entity
{
    public class HoneypotLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); 
        public string IpAddress { get; set; }
        public string AttemptedUsername { get; set; }
        public string AttemptedPassword { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
