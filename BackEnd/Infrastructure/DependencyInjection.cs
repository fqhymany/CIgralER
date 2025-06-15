using System.Net.Http.Headers;
using System.Text;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Constants;
using LawyerProject.Domain.Entities;
using LawyerProject.Infrastructure.Data;
using LawyerProject.Infrastructure.Data.Interceptors;
using LawyerProject.Infrastructure.Identity;
using LawyerProject.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace LawyerProject.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        });

        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }

    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("LawyerProjectDb");
        Guard.Against.Null(connectionString, message: "Connection string 'LawyerProjectDb' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        builder.Services.AddScoped<ICredentialService, CredentialService>();
        builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        builder.Services.AddScoped<IChatHubService, ChatHubService>();
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });

        builder.EnrichSqlServerDbContext<ApplicationDbContext>();

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddIdentityServices(builder.Configuration);

        builder.Services.AddTokenService(builder.Configuration);

        builder.Services.AddScoped<RoleManager<Role>>();
        
        builder.Services.AddHostedService<ChatCleanupService>();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddTransient<IVerification, Verification>();
        builder.Services.AddTransient<ISmsService, SmsService>();
        builder.Services.AddTransient<IEncryptionService, EncryptionService>();
        builder.Services.AddTransient<IAuditService, AuditService>();
        builder.Services.AddHttpClient<SmsService>(client =>
        {
            client.BaseAddress = new Uri("https://portal.amootsms.com/rest/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "61DAA5BF628B41E298EF39D938B7DDE6D02A3E18");
        });
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.CanPurge, policy => 
                policy.RequireRole(Roles.Administrator));
            options.AddPolicy("Agent", policy =>
                policy.RequireRole("Agent", "Admin"));
            options.AddPolicy("RegisteredUser", policy =>
                policy.RequireAuthenticatedUser());
        });

        builder.Services.AddTransient<INotificationService, FirebaseNotificationService>();

        // Add Live Chat Support Services
        builder.Services.AddScoped<IAgentAssignmentService, AgentAssignmentService>();
        builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Configure file upload limits
        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
        });

    }

    public static IServiceCollection AddTokenService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenService, TokenService>();

        // JWT Authentication configuration
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // 1. تلاش برای دریافت توکن از کوکی
                        if (context.Request.Cookies.ContainsKey("AccessToken"))
                        {
                            context.Token = context.Request.Cookies["AccessToken"];
                        }

                        // 2. در صورت عدم پیدا شدن توکن در کوکی،
                        //    تلاش برای دریافت آن از کوئری‌استرینگ برای مسیر SignalR
                        if (string.IsNullOrEmpty(context.Token))
                        {
                            var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                            var path = context.HttpContext.Request.Path;

                            // مسیر هاب SignalR خود را متناسب با MapHub تنظیم کنید
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/chathub"))
                            {
                                context.Token = accessToken;
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
