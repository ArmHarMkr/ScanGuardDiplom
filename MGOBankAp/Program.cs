using MGOBankApp.BLL.Interfaces;
using MGOBankApp.BLL.Services;
using MGOBankApp.BLL.Utilities;
using MGOBankApp.DAL.Data;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Domain.Roles;
using MGOBankApp.Hubs;
using MGOBankApp.Service.Implementations;
using MGOBankApp.Service.Interfaces;
using MGOBankApp.TelegramBot;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using ScanGuard.TelegramBot;
using Serilog;
using System.Globalization;
using Telegram.Bot;
internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

        // Настройка Serilog
        Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()  // Логируем только информационные сообщения и выше
    .WriteTo.Console()
    .WriteTo.File("Logs/app-log.txt", rollingInterval: RollingInterval.Day)
    // Исключаем логи от Microsoft и других ненужных компонентов
    .Filter.ByExcluding(logEvent => logEvent.Properties.ContainsKey("SourceContext") &&
                                    (logEvent.Properties["SourceContext"].ToString().Contains("Microsoft") ||
                                     logEvent.Properties["SourceContext"].ToString().Contains("System")))
    .CreateLogger();

        // Применяем Serilog как основной логгер для приложения
        builder.Host.UseSerilog();

        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddControllersWithViews()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

        builder.Services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { new CultureInfo("hy-AM") };
            options.DefaultRequestCulture = new RequestCulture("hy-AM");
            options.SupportedCultures = supportedCultures;
        });

        // DI Container
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient<IVulnerabilityAnalyzer, VulnerabilityAnalyzer>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IScannerService, ScannerService>();
        builder.Services.AddScoped<IScannedSites, ScannedSites>();
        builder.Services.AddHttpClient<IFileScanService, FileScanService>();
        builder.Services.AddScoped<IFileScanService, FileScanService>();
        builder.Services.AddSingleton<ITelegramBotClient>(provider =>
        {
            var token = "7927495133:AAFtVfgk6S72qcDROjDoqyfBzfmMsNrMcV0"; // ���� �����
            return new TelegramBotClient(token);
        });
        builder.Services.AddScoped<BotService>();
        builder.Services.AddScoped<TGUserService>();
        builder.Services.AddScoped<MessageSender>();
        builder.Services.AddSignalR();

        builder.Services.AddSingleton<RestSharp.RestClient>(sp =>
        {
            var options = new RestClientOptions("https://www.virustotal.com/api/v3/")
            {
                ThrowOnAnyError = true
            };
            return new RestClient(options);
        });



        var app = builder.Build();


        app.UseRequestLocalization();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.MapRazorPages();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.UseStaticFiles();

        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapAreaControllerRoute(
            name: "admin_area",
            areaName: "Admin",
            pattern: "admin/{controller=Home}/{action=Index}/{id?}");

        var logger = app.Services.GetService<ILogger<Program>>();
        logger?.LogInformation("Starting program...");

        // ������ ���� � DI
        using (var scope = app.Services.CreateScope())
        {
            var botService = scope.ServiceProvider.GetRequiredService<BotService>();
            await botService.StartAsync();  // ������� ������ ����
        }

        // �������� �����
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { SD.Role_Admin, SD.Role_Customer, SD.Role_Premium };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        // ���������� ������������ � ����� Admin
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (await userManager.FindByEmailAsync("admin@admin.com") != null)
            {
                var user = await userManager.FindByEmailAsync("admin@admin.com");
                await userManager.RemoveFromRoleAsync(user, SD.Role_Customer);
                await userManager.AddToRoleAsync(user, SD.Role_Admin);
            }
        }

        app.MapHub<ChatHub>("/chatHub");
        app.Run();
    }
}