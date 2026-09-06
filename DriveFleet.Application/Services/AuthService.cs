using DriveFleet.Application.DTOs.Auth;
using DriveFleet.Application.Exceptions;
using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Constants;
using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Services;

/// <summary>
/// Provides authentication-related application operations.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="userRepository">
    /// Repository used to access and persist user data.
    /// </param>
    /// <param name="passwordHasher">
    /// Service used to securely hash passwords.
    /// </param>
    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Registers a new client account.
    /// </summary>
    /// <param name="request">
    /// The registration data provided by the client.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// Information about the successfully registered user.
    /// </returns>
    /// <exception cref="ConflictException">
    /// Thrown when the specified email is already registered.
    /// </exception>
    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        // Normalizes user-provided values before validation and persistence.
        var firstName = request.FirstName.Trim();
        var lastName = string.IsNullOrWhiteSpace(request.LastName)
            ? null
            : request.LastName.Trim();

        var email = request.Email.Trim().ToLowerInvariant();

        var phone = string.IsNullOrWhiteSpace(request.Phone)
            ? null
            : request.Phone.Trim();

        // Prevents the creation of multiple accounts with the same email.
        var emailExists = await _userRepository.EmailExistsAsync(
            email,
            cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(
                "A user with this email already exists.");
        }

        // Generates a secure password hash before creating the user entity.
        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            PasswordHash = passwordHash,
            Provider = null,
            EmailConfirmed = false,
            RoleId = RoleIds.Client,
            CreatedAt = DateTime.UtcNow
        };

        // Persists the new user in the database.
        await _userRepository.AddAsync(
            user,
            cancellationToken);

        return new RegisterResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            EmailConfirmed = user.EmailConfirmed
        };
    }
}
