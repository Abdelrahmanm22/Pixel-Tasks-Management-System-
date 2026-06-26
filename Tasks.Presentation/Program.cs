using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Tasks.Domain;
using Tasks.Domain.Models.Identity;
using Tasks.Domain.Repositories;
using Tasks.Presentation.MappingProfiles;
using Tasks.Repository;
using Tasks.Repository.Data;

namespace Tasks.Presentation
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            #region Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()                    // Keep console for development 
                .WriteTo.File(
                    path: "Logs/log-.txt",            // Daily file name pattern
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,       // Keep last 30 days
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB per file
                    rollOnFileSizeLimit: true,
                    shared: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            #endregion
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog(); //Use Serilog as the main logging system for the entire ASP.NET Core application.

            #region Configure Services
            // Add services to the container.
            builder.Services.AddAutoMapper(M => M.AddProfiles(new List<Profile>() { new CorporationProfile() }));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation().AddViewOptions(options => { 
                options.HtmlHelperOptions.ClientValidationEnabled = true;
            });
            builder.Services.AddDbContext<TaskContext>(Options =>
            {
                Options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<TaskContext>();
            #endregion
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            #region Update-Database-on-Startup
            using var Scope = app.Services.CreateScope();
            var Services = Scope.ServiceProvider;
            try
            {
                var DbContext = Services.GetRequiredService<TaskContext>();
                await DbContext.Database.MigrateAsync();//Udpate-Database on Startup

                #region Data Seeding
                var roleManager = Services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = Services.GetRequiredService<UserManager<AppUser>>();
                await AppIdentityDbContextSeed.SeedUserAsync(userManager, roleManager);
                #endregion
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while applying database migrations.");
            }
            #endregion

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
