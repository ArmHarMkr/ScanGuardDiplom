using Telegram.Bot;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using Microsoft.Extensions.Hosting;
using ScanGuard.TelegramBot.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace ScanGuard.TelegramBot
{
    internal class Program
    {

        public static async Task Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Подключаем все необходимые сервисы
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer("Your_Connection_String")); // Подключение к БД

                    services.AddSingleton<ITelegramBotClient>(new TelegramBotClient("7927495133:AAFtVfgk6S72qcDROjDoqyfBzfmMsNrMcV0")); // Бот
                 //   services.AddScoped<TGUserService>(); // Сервис пользователей для работы с БД
                    services.AddSingleton<BotService>(); // Сервис, управляющий ботом
                })
                .Build();

            // Получаем и запускаем BotService
            var botService = host.Services.GetRequiredService<BotService>();
            await botService.StartAsync();

            Console.WriteLine("Бот запущен. Нажмите Enter для выхода...");
            Console.ReadLine();
        }
        private static async Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {

        }
        private static async Task Error(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            Console.WriteLine(exception.Message);
        }
    }
}
