using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ScanGuard.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;


        public ChatHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task SendMessage(string user, string message, string profilePhoto)
        {
            ApplicationUser? appUser = await _context.Users.FirstOrDefaultAsync(x => x.FullName == user);
            var chatMessage = new ChatMessageEntity
            {
                UserName = user,
                UserId = appUser?.Id,
                Message = message
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync(); // Save to DB

            await Clients.All.SendAsync("ReceiveMessage", user, message, profilePhoto);
        }
    }

}
