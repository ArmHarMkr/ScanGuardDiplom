using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using MGOBankApp.BLL.Services;
using System.ComponentModel.DataAnnotations;

namespace ScanGuard.TelegramBot
{
    public class BotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly TGUserService _userService;

        public BotService(ITelegramBotClient botClient, TGUserService userService)
        {
            _botClient = botClient;
            _userService = userService;
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
                if (message.Type == MessageType.Text)
                {
                    var text = message.Text;
                    if (text == "/start")
                    {
                        await client.SendMessage(message.Chat.Id, "Hello !!!\r\nIf you want to connect your ScanGuard account to the bot, then use the command \r\n<b>/connect [Your tg token]</b>\r\n You can get it in the Get Token section✅", cancellationToken: token, parseMode: ParseMode.Html);
                    }
                    else if (text == "/connect")
                    {
                        await client.SendMessage(message.Chat.Id, "Please, use\r\n<b>/connect [Your tg token]</b>", cancellationToken: token, parseMode: ParseMode.Html);
                    }
                    else if (text == "/scanurl")
                    {
                        await client.SendMessage(message.Chat.Id, "Please, use\r\n<b>/scanurl [url]</b>\r\n(ex. https://example.com)", cancellationToken: token, parseMode: ParseMode.Html);
                    }
                    else if (text == "/disconnect")
                    {
                        var result = await _userService.DisconnectUser(message.Chat.Id.ToString());
                        await client.SendMessage(message.Chat.Id, result, cancellationToken: token);
                    }
                    else if (text!.StartsWith("/connect"))
                    {
                        var tgToken = text.Split(" ")[1];
                        var result = await _userService.ConnectUser(tgToken, message.Chat.Id.ToString());

                        await client.SendMessage(message.Chat.Id, result, cancellationToken: token);
                    }
                    else if (text!.StartsWith("/scanurl"))
                    {
                        var result = await _userService.ScanUrl(text.Split(" ")[1], message.Chat.Id.ToString());
                        await client.SendMessage(message.Chat.Id, result, cancellationToken: token, parseMode: ParseMode.Html);
                    }else if (text == "/profile")
                    {
                        var result = await _userService.GetProfileInfo(message.Chat.Id.ToString());
                        await client.SendPhoto(message.Chat.Id, new InputFileStream(File.OpenRead(result.profileImageUrl)), caption: result.profileInfo,cancellationToken: token);
                    }
                    
                }
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}