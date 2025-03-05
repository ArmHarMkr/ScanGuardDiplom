using MGOBankApp.BLL.Interfaces;
using MGOBankApp.DAL.Interfaces;
using MGOBankApp.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.BLL.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task AddMessageAsync(ApplicationUser user, string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                var message = new MessageEntity
                {
                    ApplicationUser = user,
                    Content = content,
                    DateTime = DateTime.Now
                };

                await _chatRepository.AddMessageAsync(message);
            }
        }

        public List<MessageEntity> GetAllMessages()
        {
            return _chatRepository.GetAllMessages();
        }

        public async Task CleanupOldMessagesAsync()
        {
            await _chatRepository.DeleteOldMessagesAsync();
        }

    }
}
