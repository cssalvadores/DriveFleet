using System.Net;
using DriveFleet.Application.DTOs.Auth;
using DriveFleet.Application.Exceptions;
using DriveFleet.Application.Interfaces;
using DriveFleet.Application.Options;
using DriveFleet.Domain.Constants;
using DriveFleet.Domain.Entities;
using Microsoft.Extensions.Options;

namespace DriveFleet.Application.Services;

/// <summary>
/// Provides authentication-related application operations.
/// </summary>
public class AuthService : IAuthService
{
    private static readonly TimeSpan EmailConfirmationTokenLifetime =
        TimeSpan.FromHours(24);

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecureTokenService _secureTokenService;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEmailConfirmationTokenRepository
        _emailConfirmationTokenRepository;
    private readonly IEmailSender _emailSender;
    private readonly ApplicationUrlSettings _applicationUrls;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="userRepository">
    /// Repository used to query user data.
    /// </param>
    /// <param name="passwordHasher">
    /// Service used to securely hash passwords.
    /// </param>
    /// <param name="secureTokenService">
    /// Service used to generate and hash secure tokens.
    /// </param>
    /// <param name="registrationRepository">
    /// Repository used to persist registration data atomically.
    /// </param>
    /// <param name="emailConfirmationTokenRepository">
    /// Repository used to retrieve and update email confirmation tokens.
    /// </param>
    /// <param name="emailSender">
    /// Service used to send application emails.
    /// </param>
    /// <param name="applicationUrlOptions">
    /// Application URL configuration used to build confirmation links.
    /// </param>
    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISecureTokenService secureTokenService,
        IRegistrationRepository registrationRepository,
        IEmailConfirmationTokenRepository emailConfirmationTokenRepository,
        IEmailSender emailSender,
        IOptions<ApplicationUrlSettings> applicationUrlOptions)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _secureTokenService = secureTokenService;
        _registrationRepository = registrationRepository;
        _emailConfirmationTokenRepository =
            emailConfirmationTokenRepository;
        _emailSender = emailSender;
        _applicationUrls = applicationUrlOptions.Value;
    }

    /// <summary>
    /// Registers a new client account and sends an email confirmation link.
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

        var email = request.Email
            .Trim()
            .ToLowerInvariant();

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

        // Generates a secure password hash before creating the user.
        var passwordHash = _passwordHasher.Hash(
            request.Password);

        var now = DateTime.UtcNow;

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            PasswordHash = passwordHash,
            Photo = null,
            Provider = null,
            EmailConfirmed = false,
            RoleId = RoleIds.Client,
            CreatedAt = now,
            UpdatedAt = null
        };

        // Generates the raw email confirmation token.
        var rawToken = _secureTokenService.GenerateToken();

        // Only the hash of the token will be persisted.
        var tokenHash = _secureTokenService.HashToken(
            rawToken);

        var confirmationToken = new EmailConfirmationToken
        {
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAt = now.Add(
                EmailConfirmationTokenLifetime),
            ConfirmedAt = null
        };

        // Persists the user and confirmation token atomically.
        await _registrationRepository
            .AddUserWithConfirmationTokenAsync(
                user,
                confirmationToken,
                cancellationToken);

        // Builds the confirmation URL sent to the user.
        var confirmationUrl =
            $"{_applicationUrls.WebBaseUrl.TrimEnd('/')}" +
            "/account/confirm-email?token=" +
            Uri.EscapeDataString(rawToken);

        // Encodes user-provided content before including it in HTML.
        var safeFirstName =
            WebUtility.HtmlEncode(user.FirstName);

        var safeConfirmationUrl =
            WebUtility.HtmlEncode(confirmationUrl);

        var emailSubject =
            "Confirm your DriveFleet account";

        var emailBody = $"""
            <h2>Welcome to DriveFleet</h2>

            <p>Hello {safeFirstName},</p>

            <p>
                Please confirm your email address by clicking
                the link below:
            </p>

            <p>
                <a href="{safeConfirmationUrl}">
                    Confirm email
                </a>
            </p>

            <p>
                This confirmation link is valid for 24 hours.
            </p>
            """;

        // Sends the raw token only through the confirmation link.
        await _emailSender.SendAsync(
            user.Email,
            emailSubject,
            emailBody,
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

    /// <summary>
    /// Confirms a user's email address using a confirmation token.
    /// </summary>
    /// <param name="token">
    /// The raw email confirmation token received from the client.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <exception cref="InvalidTokenException">
    /// Thrown when the confirmation token is invalid or expired.
    /// </exception>
    /// <exception cref="ConflictException">
    /// Thrown when the confirmation token has already been used
    /// or the associated email address is already confirmed.
    /// </exception>
    public async Task ConfirmEmailAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        // Rejects missing or empty confirmation tokens.
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidTokenException(
                "The email confirmation token is invalid.");
        }

        // Hashes the raw token so it can be compared with the stored hash.
        var tokenHash = _secureTokenService.HashToken(
            token.Trim());

        // Retrieves the confirmation token and its associated user.
        var confirmationToken =
            await _emailConfirmationTokenRepository
                .GetByTokenHashAsync(
                    tokenHash,
                    cancellationToken);

        // Rejects tokens that do not exist in the database.
        if (confirmationToken is null)
        {
            throw new InvalidTokenException(
                "The email confirmation token is invalid.");
        }

        // Prevents an already confirmed account from being confirmed again.
        if (confirmationToken.ConfirmedAt.HasValue ||
            confirmationToken.User.EmailConfirmed)
        {
            throw new ConflictException(
                "The email address has already been confirmed.");
        }

        var now = DateTime.UtcNow;

        // Rejects confirmation tokens whose validity period has ended.
        if (confirmationToken.ExpiresAt <= now)
        {
            throw new InvalidTokenException(
                "The email confirmation token has expired.");
        }

        // Marks both the token and the associated user as confirmed.
        await _emailConfirmationTokenRepository.ConfirmAsync(
            confirmationToken,
            now,
            cancellationToken);
    }
}
