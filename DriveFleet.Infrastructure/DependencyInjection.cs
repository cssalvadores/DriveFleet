using DriveFleet.Application.Interfaces;
using DriveFleet.Infrastructure.Data;
using DriveFleet.Infrastructure.Email;
using DriveFleet.Infrastructure.Repositories;
using DriveFleet.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DriveFleet.Infrastructure;

/// <summary>
/// Registers infrastructure services required by DriveFleet.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        // Registers the DriveFleet database context using SQL Server.
        services.AddDbContext<DriveFleetDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Registers application repositories.
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<
            IEmailConfirmationTokenRepository,
            EmailConfirmationTokenRepository>();

        services.AddScoped<
            IRegistrationRepository,
            RegistrationRepository>();

        // Registers security services.
        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddSingleton<ISecureTokenService, SecureTokenService>();

        // Registers SMTP configuration and email delivery.
        services.Configure<SmtpSettings>(
            configuration.GetSection("Smtp"));

        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
