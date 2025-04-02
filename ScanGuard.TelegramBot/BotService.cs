using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using ScanGuard.BLL.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using ScanGuard.TelegramBot;

namespace ScanGuard.TelegramBot
{
    public class BotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly MessageSender _sender;
        private readonly ILogger<BotService> _logger;

        public BotService(ITelegramBotClient botClient, MessageSender sender,ILogger<BotService> logger)
        {
            _botClient = botClient;
            _sender = sender;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // Получаем все обновления
            };

            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                if (message!.Type == MessageType.Text)
                {
                    _sender.SendMessage(client, message, token);
                   _logger.LogInformation("Telegram {ChatId} : {Message}", message.Chat.Id,message.Text);
                }
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            _logger.LogError("Telegram Error: {Message}",exception.Message);
            return Task.CompletedTask;
        }
    }
}