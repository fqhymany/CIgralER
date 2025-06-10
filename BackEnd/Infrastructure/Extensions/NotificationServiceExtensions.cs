using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LawyerProject.Infrastructure.Extensions;

public static class NotificationServiceExtensions
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // تغییر از Singleton به Scoped
        services.AddScoped<INotificationService, FirebaseNotificationService>();

        // Firebase SDK میتواند همچنان Singleton باشد
        services.AddSingleton(FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(
                configuration["Firebase:ServiceAccountKeyPath"])
        }));

        return services;
    }
}
