using DriveFleet.Application.Interfaces;
using DriveFleet.Application.Options;
using DriveFleet.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DriveFleet.Application;

/// <summary>
/// Provides dependency injection registration for application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers DriveFleet application services and application-level configuration.
    /// </summary>
    /// <param name="services">
    /// The service collection used to register application dependencies.
    /// </param>
    /// <param name="configuration">
    /// The application configuration used to bind application settings.
    /// </param>
    /// <returns>
    /// The same service collection so additional registrations can be chained.
    /// </returns>
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Binds the "ApplicationUrls" configuration section
        // to the ApplicationUrlSettings class.
        services.Configure<ApplicationUrlSettings>(
            configuration.GetSection("ApplicationUrls"));

        // Registers the authentication application service
        // for the lifetime of the current HTTP request.
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
