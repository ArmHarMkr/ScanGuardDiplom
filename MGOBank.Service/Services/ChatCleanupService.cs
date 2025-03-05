using MGOBankApp.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.BLL.Services
{
    public class ChatCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ChatCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var chatService = scope.ServiceProvider.GetRequiredService<IChatService>(); // Use Interface
                    await chatService.CleanupOldMessagesAsync();
                }
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }

}
