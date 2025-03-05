using MGOBankApp.BLL.Interfaces;
using MGOBankApp.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace MGOBankApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(IChatService chatService, UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
        }

        public async Task SendMessage(ApplicationUser applicationUser, string message)
        {
            if (applicationUser == null) return;

            await _chatService.AddMessageAsync(applicationUser, message);
            await Clients.All.SendAsync("ReceiveMessage", applicationUser.FullName, message, DateTime.Now.ToShortTimeString());
        }
    }
}
