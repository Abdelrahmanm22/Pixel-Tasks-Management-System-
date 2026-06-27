using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Tasks.Domain;
using Tasks.Domain.Models.Identity;
using Tasks.Domain.Repositories;
using Tasks.Domain.Services;
using Tasks.Presentation.Authorization;
using Tasks.Presentation.MappingProfiles;
using Tasks.Repository;
using Tasks.Repository.Data;
using Tasks.Services.CodeGeneration;

namespace Tasks.Presentation
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            #region Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            #endregion

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();

            #region Configure Services
            builder.Services.AddAutoMapper(M => M.AddProfiles(new List<Profile>() { new CorporationProfile(), new TaskTypeProfile(), new SectionProfile(), new WorkTaskProfile() }));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();
            builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddViewOptions(options =>
                {
                    options.HtmlHelperOptions.ClientValidationEnabled = true;
                });

            builder.Services.AddDbContext<TaskContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                // Password policy
                options.Password.RequireDigit           = true;
                options.Password.RequiredLength         = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase       = true;
                options.Password.RequireLowercase       = true;
            })
            .AddEntityFrameworkStores<TaskContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>();

            // Authorization
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddAuthorization();

            // Configure cookie to redirect to Login when unauthenticated
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath        = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan   = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
            });
            #endregion

            var app = builder.Build();

            #region Update-Database-on-Startup
            using var scope    = app.Services.CreateScope();
            var services       = scope.ServiceProvider;
            try
            {
                var dbContext = services.GetRequiredService<TaskContext>();
                await dbContext.Database.MigrateAsync();

                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                await AppIdentityDbContextSeed.SeedUserAsync(userManager, roleManager);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while applying database migrations.");
            }
            #endregion

            // ─── Middleware Pipeline ──────────────────────────────────────────
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication(); // Must come before UseAuthorization
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                // Start on the Login page by default
                pattern: "{controller=Account}/{action=Login}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
