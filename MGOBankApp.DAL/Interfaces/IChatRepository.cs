using MGOBankApp.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.DAL.Interfaces
{
    public interface IChatRepository
    {
        public Task AddMessageAsync(MessageEntity chatEntity);
        public List<MessageEntity> GetAllMessages();
        public Task DeleteOldMessagesAsync();
    }
}
