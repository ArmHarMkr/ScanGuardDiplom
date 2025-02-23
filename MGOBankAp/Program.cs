using Microsoft.EntityFrameworkCore;
using MGOBankApp.Domain.Entity;
using MGOBankApp.DAL.Data;
using Microsoft.AspNetCore.Identity;
using MGOBankApp.Domain.Roles;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using MGOBankApp.Service.Interfaces;
using MGOBankApp.Service.Implementations;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();


        builder.Services.AddControllersWithViews()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
        new CultureInfo("hy-AM")
            };

            options.DefaultRequestCulture = new RequestCulture("hy-AM");
            options.SupportedCultures = supportedCultures;
        });

        //DI Container
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IScannerService, ScannerService>();


        var app = builder.Build();
        app.UseRequestLocalization();


        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.MapRazorPages();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapAreaControllerRoute(
            name: "admin_area",
            areaName: "Admin",
            pattern: "Admin/{controller=Home}/{action=Index}/{id?}"
        );



        var logger = app.Services.GetService<ILogger<Program>>();
        logger?.LogInformation("Starting program...");


        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { SD.Role_Admin, SD.Role_Customer };

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
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (await userManager.FindByEmailAsync("admin@admin.com") != null)
            {
                var user = await userManager.FindByEmailAsync("admin@admin.com");
                await userManager.RemoveFromRoleAsync(user, SD.Role_Customer);
                await userManager.AddToRoleAsync(user, SD.Role_Admin);
            }
        }

        app.Run();
    }
}