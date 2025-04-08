using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.Domain.Entity
{
    public class ReviewEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ApplicationUser User { get; set; }
        public string ReviewText { get; set; }  
        public bool IsGood { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}
