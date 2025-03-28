using ScanGuard.BLL.Interfaces;
using ScanGuard.BLL.Services;
using ScanGuard.BLL.Utilities;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using ScanGuard.Domain.Roles;
using ScanGuard.Hubs;
using ScanGuard.Service.Implementations;
using ScanGuard.Service.Interfaces;
using ScanGuard.TelegramBot;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using ScanGuard.BLL.Interfaces;
using ScanGuard.BLL.Services;
using ScanGuard.TelegramBot;
using Serilog;
using System.Globalization;
using Telegram.Bot;
using Microsoft.AspNetCore.Diagnostics;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

Log.Logger = new LoggerConfiguration()
.MinimumLevel.Information()
.WriteTo.Console()
.WriteTo.File("Logs/app-log.txt", rollingInterval: RollingInterval.Day)
.Filter.ByExcluding(logEvent => logEvent.Properties.ContainsKey("SourceContext") &&
                                (logEvent.Properties["SourceContext"].ToString().Contains("Microsoft") ||
                                 logEvent.Properties["SourceContext"].ToString().Contains("System")))
.CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
builder.Services.AddControllers();

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
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddHttpClient<IFileScanService, FileScanService>();

builder.Services.AddScoped<IFileScanService, FileScanService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<ICorpService, CorpService>();
builder.Services.AddSingleton<ITelegramBotClient>(provider =>
{
    var token = "7927495133:AAFtVfgk6S72qcDROjDoqyfBzfmMsNrMcV0";
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
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            Log.Error(error.Error, "Error on {Path}", context.Request.Path);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Server error");
        }
    });
});

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "admin_area",
    areaName: "Admin",
    pattern: "admin/{controller=Home}/{action=Index}/{id?}");


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapControllers();
});


var logger = app.Services.GetService<ILogger<Program>>();
logger?.LogInformation("Starting program...");

using (var scope = app.Services.CreateScope())
{
    var botService = scope.ServiceProvider.GetRequiredService<BotService>();
    await botService.StartAsync();  
}

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

using (var scope = app.Services.CreateScope())
{
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string adminEmail = configuration["AdminSettings:AdminEmail"]!;

    if (!string.IsNullOrEmpty(adminEmail))
    {
        var user = await userManager.FindByEmailAsync(adminEmail);
        if (user != null)
        {
            await userManager.RemoveFromRoleAsync(user, SD.Role_Customer);
            await userManager.AddToRoleAsync(user, SD.Role_Admin);
        }
    }
}

app.MapHub<ChatHub>("/chatHub");
app.Run();
