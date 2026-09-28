using System.Text.Json;
using DriveFleet.Mobile.Models.Auth;
using Microsoft.Extensions.Logging;

namespace DriveFleet.Mobile.Services;

/// <summary>
/// Manages the authenticated employee session
/// using the platform secure storage.
/// </summary>
public class SessionService
{
    private const string SessionStorageKey =
        "drivefleet.employee.session";

    private const string EmployeeRole =
        "Employee";

    private readonly ILogger<SessionService> _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SessionService"/> class.
    /// </summary>
    public SessionService(
        ILogger<SessionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Saves a successful employee authentication
    /// response in secure platform storage.
    /// </summary>
    public async Task<bool> SaveAsync(
        LoginResponse login)
    {
        try
        {
            var session =
                new EmployeeSession
                {
                    UserId =
                        login.UserId,

                    FirstName =
                        login.FirstName,

                    LastName =
                        login.LastName,

                    Email =
                        login.Email,

                    Role =
                        login.Role,

                    AccessToken =
                        login.AccessToken,

                    ExpiresAt =
                        login.ExpiresAt
                };

            var json =
                JsonSerializer.Serialize(
                    session);

            await SecureStorage.Default.SetAsync(
                SessionStorageKey,
                json);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to save the employee session securely.");

            return false;
        }
    }

    /// <summary>
    /// Retrieves the current valid employee session
    /// from secure platform storage.
    /// </summary>
    public async Task<EmployeeSession?> GetAsync()
    {
        try
        {
            var json =
                await SecureStorage.Default.GetAsync(
                    SessionStorageKey);

            if (string.IsNullOrWhiteSpace(
                json))
            {
                return null;
            }

            var session =
                JsonSerializer.Deserialize<EmployeeSession>(
                    json);

            if (!IsValid(
                session))
            {
                Clear();

                return null;
            }

            return session;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to retrieve the employee session.");

            Clear();

            return null;
        }
    }

    /// <summary>
    /// Retrieves the JWT access token from the
    /// current valid employee session.
    /// </summary>
    public async Task<string?> GetAccessTokenAsync()
    {
        var session =
            await GetAsync();

        return session?.AccessToken;
    }

    /// <summary>
    /// Removes the authenticated employee session
    /// from secure platform storage.
    /// </summary>
    public void Clear()
    {
        try
        {
            SecureStorage.Default.Remove(
                SessionStorageKey);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Unable to remove the employee session.");
        }
    }

    /// <summary>
    /// Determines whether a stored employee session
    /// contains the required authentication information.
    /// </summary>
    private static bool IsValid(
        EmployeeSession? session)
    {
        if (session is null)
        {
            return false;
        }

        if (session.UserId <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(
            session.Email))
        {
            return false;
        }

        if (!string.Equals(
            session.Role,
            EmployeeRole,
            StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(
            session.AccessToken))
        {
            return false;
        }

        return session.ExpiresAt >
            DateTime.UtcNow;
    }
}
