using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.BLL.Services
{
    public class TGUserService
    {
        private readonly ApplicationDbContext _context;

        public TGUserService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<string> LinkUser(string chatId, string token)
        {
            var user = await _context.TGUserEntities.FirstOrDefaultAsync(x => x.TGUserToken == token);
            if (user == null)
            {
                return "Token is not valid";
            }
            else if (user.TGUserId is not null)
            {
                return "Token already linked";
            }
            //todo
            user.TGUserId = chatId;
            user.TGConnectedTime = DateTime.Now;
            user.ApplicationUser.TGConnected = true;

            await _context.SaveChangesAsync();
            return "Connected";
        }
    }
}
