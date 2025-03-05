using MGOBankApp.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.BLL.Interfaces
{
    public interface IChatService
    {
        public Task AddMessageAsync(ApplicationUser user, string message);
        public List<MessageEntity> GetAllMessages();
        public Task CleanupOldMessagesAsync();
    }
}
