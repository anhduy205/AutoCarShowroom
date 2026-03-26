using AutoCarShowroom.Data;
using AutoCarShowroom.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var seedOnly = args.Contains("--seed-only", StringComparer.OrdinalIgnoreCase);
            var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
            var dataProtectionDirectory = Path.Combine(dataDirectory, "DataProtectionKeys");

            Directory.CreateDirectory(dataDirectory);
            Directory.CreateDirectory(dataProtectionDirectory);

            AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionDirectory))
                .SetApplicationName("AutoCarShowroom");

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddControllersWithViews();
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".AutoCarShowroom.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromHours(4);
            });

            builder.Services.Configure<AdminAccountOptions>(builder.Configuration.GetSection("AdminAccount"));
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                });
            builder.Services.AddAuthorization();

            var connectionStringTemplate = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing DefaultConnection.");
            var connectionString = connectionStringTemplate.Replace("|DataDirectory|", dataDirectory, StringComparison.OrdinalIgnoreCase);

            builder.Services.AddDbContext<ShowroomDbContext>(options =>
                options.UseSqlite(connectionString));

            var app = builder.Build();

            Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath, "uploads", "cars"));

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var dbContext = services.GetRequiredService<ShowroomDbContext>();
                    await dbContext.Database.EnsureCreatedAsync();
                    await ShowroomDemoSeeder.InitializeAsync(services);

                    if (seedOnly)
                    {
                        var carCount = await dbContext.Cars.CountAsync();
                        var brandCount = await dbContext.Cars
                            .Select(car => car.Brand)
                            .Distinct()
                            .CountAsync();
                        var statusSummary = await dbContext.Cars
                            .GroupBy(car => car.Status)
                            .Select(group => new { Status = group.Key, Count = group.Count() })
                            .OrderBy(item => item.Status)
                            .ToListAsync();

                        Console.WriteLine($"SeedSummary Cars={carCount} Brands={brandCount}");
                        Console.WriteLine($"StatusSummary {string.Join(", ", statusSummary.Select(item => $"{item.Status}:{item.Count}"))}");
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning(ex, "Unable to initialize database during startup.");
                }
            }

            if (seedOnly)
            {
                return;
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}
