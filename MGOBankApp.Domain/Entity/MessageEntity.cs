using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.Domain.Entity
{
    public class MessageEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ApplicationUser ApplicationUser { get; set; }
        public string Content { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
    }

}
