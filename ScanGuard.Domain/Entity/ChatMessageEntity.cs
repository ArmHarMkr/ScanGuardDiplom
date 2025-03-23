using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.Domain.Entity
{
    public class ChatMessageEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? UserId { get; set; } 
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;

    }
}
