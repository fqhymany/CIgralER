using Azure.Identity;
using LawyerProject.Application.CaseFinancials.Overview.Interfaces;
using LawyerProject.Application.Cases.Services;
using LawyerProject.Application.Cases.Services.CaseFinancial;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Infrastructure.Identity;
using LawyerProject.Infrastructure.Services;
using LawyerProject.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;

namespace LawyerProject.Web;
public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddMemoryCache();

        builder.Services.AddScoped<IUser, CurrentUser>();
        builder.Services.AddScoped<IClaimsTransformation, UserClaimsTransformation>();
        builder.Services.AddScoped<IRegionService, RegionService>();
        builder.Services.AddScoped<IAuthorizationCacheService, AuthorizationCacheService>();
        builder.Services.AddScoped<IRegionFilter, RegionFilter>();
        builder.Services.AddScoped<ICaseInfoResolver, CaseInfoResolver>();
        builder.Services.AddScoped<ICaseFinancialOverviewCalculator, OverviewFinancialCalculator>();
        builder.Services.AddScoped<ICaseAccessFilter, CaseAccessFilter>();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();

        builder.Services.AddRazorPages();

        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "LawyerProject API";
            configure.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Type = OpenApiSecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));

        });
        builder.Services.Configure<GallerySettings>(builder.Configuration.GetSection("GallerySettings"));
        
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ReactApp", corsPolicyBuilder =>
                    corsPolicyBuilder
                        //.SetIsOriginAllowed(origin =>
                        //{
                        //    if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                        //    {
                        //        // به عنوان مثال: درخواست‌هایی که host برابر با localhost یا دارای پسوند .localhost هستند را مجاز می‌کند
                        //        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                        //               uri.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) ||
                        //               uri.Host.Equals("dadvik", StringComparison.OrdinalIgnoreCase) ||
                        //               uri.Host.EndsWith(".dadvik", StringComparison.OrdinalIgnoreCase) ||
                        //               uri.Host.Equals("192.168.1.10", StringComparison.OrdinalIgnoreCase) ||
                        //               uri.Host.EndsWith(".192.168.1.10", StringComparison.OrdinalIgnoreCase) ||
                        //               uri.Host.Equals("1.0.0.10", StringComparison.OrdinalIgnoreCase) ||
                        //               uri.Host.EndsWith(".1.0.0.10", StringComparison.OrdinalIgnoreCase);

                        //    }
                        //    return false;
                        //})
                        .SetIsOriginAllowed(origin => true).WithOrigins("http://localhost:44447", "http://localhost:5000", "http://localhost:5001", "http://192.168.1.10:120", "https://192.168.1.10", "http://dadvik.ir", "https://dadvik.ir")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
        });

    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
