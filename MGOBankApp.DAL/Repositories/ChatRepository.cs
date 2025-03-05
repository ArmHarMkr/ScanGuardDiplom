using MGOBankApp.DAL.Data;
using MGOBankApp.DAL.Interfaces;
using MGOBankApp.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.DAL.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddMessageAsync(MessageEntity message)
        {
            _context.MessageEntities.Add(message);
            await _context.SaveChangesAsync();
        }

        public List<MessageEntity> GetAllMessages()
        {
            return _context.MessageEntities.OrderBy(m => m.DateTime).Include(x => x.ApplicationUser).ToList();
        }

        public async Task DeleteOldMessagesAsync()
        {
            var twoWeeksAgo = DateTime.Now.AddDays(-14);
            var oldMessages = _context.MessageEntities.Where(m => m.DateTime < twoWeeksAgo);
            _context.MessageEntities.RemoveRange(oldMessages);
            await _context.SaveChangesAsync();
        }
    }
}
