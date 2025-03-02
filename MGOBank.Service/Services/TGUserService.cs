using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace MGOBankApp.BLL.Services
{
    public class TGUserService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TGUserService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<string> LinkUser(string token, string chatId)
        {
<<<<<<< HEAD
            var user =  await _context.TGUserEntities.FirstOrDefaultAsync(x => x.TGUserToken == token);
=======
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = await _context.TGUserEntities.Include(x => x.ApplicationUser).FirstOrDefaultAsync(x => x.TGUserToken == token);
>>>>>>> e7e61456fa1c6a49e807eabeb94c6cd92121a0b2
            if (user == null)
            {
                return "Token is not valid";
            }
            else if (user.TGUserId is not null)
            {
                return "Token already linked";
            }

            user.TGUserId = chatId;
            user.TGConnectedTime = DateTime.Now;
            user.ApplicationUser.TGConnected = true;

            await _context.SaveChangesAsync();
            return "Connected";
        }
    }
}