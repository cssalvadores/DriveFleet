using DriveFleet.Application.Interfaces;
using DriveFleet.Infrastructure.Data;
using DriveFleet.Infrastructure.Repositories;
using DriveFleet.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DriveFleet.Infrastructure;

/// <summary>
/// Registers infrastructure services required by DriveFleet.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        // Registers the DriveFleet database context using SQL Server.
        services.AddDbContext<DriveFleetDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Registers application repositories.
        services.AddScoped<IUserRepository, UserRepository>();

        // Registers the password hashing service.
        services.AddScoped<IPasswordHasher, PasswordHasherService>();

        return services;
    }
}
