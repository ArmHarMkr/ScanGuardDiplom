using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using MGOBankApp.Service.Implementations;
using Microsoft.Identity.Client;
using MGOBankApp.Service.Interfaces;
using MGOBankApp.BLL.Services;

namespace MGOBankApp.TelegramBot
{
    public class MessageSender
    {
        public readonly TGUserService _userService;
        public MessageSender(TGUserService userService)
        {
            _userService = userService;
        }
        internal async Task SendMessage(ITelegramBotClient client, Message message, CancellationToken token = default) {

            var text = message.Text;
            var chatId = message.Chat.Id;
            if (text == "/start")
            {
                await client.SendMessage(chatId, "Hello !!!\r\nIf you want to connect your ScanGuard account to the bot, then use the command \r\n<b>/connect [Your tg token]</b>\r\n You can get it in the Get Token section✅", cancellationToken: token, parseMode: ParseMode.Html);
            }
            else if (text == "/connect")
            {
                await client.SendMessage(chatId, "Please, use\r\n<b>/connect [Your tg token]</b>", cancellationToken: token, parseMode: ParseMode.Html);
            }
            else if (text == "/scanurl")
            {
                await client.SendMessage(chatId, "Please, use\r\n<b>/scanurl [url]</b>\r\n(ex. https://example.com)", cancellationToken: token, parseMode: ParseMode.Html);
            }
            else if (text == "/disconnect")
            {
                var result = await _userService.DisconnectUser(chatId.ToString());
                await client.SendMessage(chatId, result, cancellationToken: token);
            }
            else if (text!.StartsWith("/connect"))
            {
                var tgToken = text.Split(" ")[1];
                var result = await _userService.ConnectUser(tgToken, chatId.ToString());

                await client.SendMessage(chatId, result, cancellationToken: token);
            }
            else if (text!.StartsWith("/scanurl"))
            {
                var result = await _userService.ScanUrl(text.Split(" ")[1], chatId.ToString());
                await client.SendMessage(chatId, result, cancellationToken: token, parseMode: ParseMode.Html);
            }
            else if (text == "/profile")
            {
                var result = await _userService.GetProfileInfo(chatId.ToString());
                if (result.profileImageUrl == null)
                {
                    await client.SendMessage(chatId, result.profileInfo, cancellationToken: token, parseMode: ParseMode.Html);
                }
                else
                {
                    await client.SendPhoto(chatId, new InputFileStream(File.OpenRead(result.profileImageUrl!)), caption: result.profileInfo, cancellationToken: token, parseMode: ParseMode.Html);
                }
            }else if (text == "/help")
            {
                await client.SendMessage(
    chatId,
    """
    🤖 <b>Welcome to ScanGuard Bot!</b> 🚀

    This bot is designed to help you secure your website and protect it from vulnerabilities. With ScanGuard, you can scan your website for potential security threats such as SQL Injection, XSS, and CSRF attacks. 

    Here are the commands you can use to interact with the bot:

    🌐 <b>/start</b> - Start the bot and get basic information.
    🔗 <b>/connect [Your tg token]</b> - Connect your ScanGuard account to the bot and start scanning.
    🖥️ <b>/scanurl [url]</b> - Scan a URL for security issues and get detailed results.
    🔓 <b>/disconnect</b> - Disconnect your account from the bot and stop receiving updates.
    👤 <b>/profile</b> - View your profile information, including the status of your account and scans.

    📤 To scan a file, simply send the file to the bot and we'll analyze it for security issues! 📂

    💬 <b>Need help or have questions?</b> 
    - Our support team is here to assist you! Feel free to reach out to us directly via Telegram:

    <b>Support:</b>  
    📱 <b>Suggestions / bugs</b> - @har_mkr 
    📱 <b>Technical support</b> - @davlive

    🕐 Hours: 9 AM - 6 PM (Mon - Fri)

    We're here to help you stay secure! 🔒
    """,
    cancellationToken: token,
    parseMode: ParseMode.Html
);
            }
            else
            {
                await client.SendMessage(
    chatId,
    """
    🚫 <b>Sorry, I didn't understand that command!</b>

    Here are some commands you can use:

    <b>/connect [Your tg token]</b> - Connect your ScanGuard account 🔗
    <b>/scanurl [url]</b> - Scan a URL for security issues 🌐
    <b>/disconnect</b> - Disconnect your account from the bot 🔓
    <b>/profile</b> - View your profile information 👤

    If you need help, just type <b>/help</b> 🤖
    """,
    cancellationToken: token,
    parseMode: ParseMode.Html
);
            }
        }
    }
}
