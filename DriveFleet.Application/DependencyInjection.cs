using DriveFleet.Application.Interfaces;
using DriveFleet.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DriveFleet.Application;

/// <summary>
/// Provides dependency injection registration for application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers DriveFleet application services.
    /// </summary>
    /// <param name="services">
    /// The service collection used by the application.
    /// </param>
    /// <returns>
    /// The same service collection so additional registrations can be chained.
    /// </returns>
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // Registers authentication-related application operations.
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
