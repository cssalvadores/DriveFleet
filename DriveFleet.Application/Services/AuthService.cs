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

    private static readonly TimeSpan PasswordResetTokenLifetime =
    TimeSpan.FromMinutes(30);

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecureTokenService _secureTokenService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEmailConfirmationTokenRepository
    _emailConfirmationTokenRepository;
    private readonly IPasswordResetTokenRepository
    _passwordResetTokenRepository;
    private readonly IRevokedJwtTokenRepository
    _revokedJwtTokenRepository;
    private readonly IEmailSender _emailSender;
    private readonly ApplicationUrlSettings _applicationUrls;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="userRepository">
    /// Repository used to query user data.
    /// </param>
    /// <param name="passwordHasher">
    /// Service used to securely hash and verify passwords.
    /// </param>
    /// <param name="secureTokenService">
    /// Service used to generate and hash secure tokens.
    /// </param>
    /// <param name="jwtTokenService">
    /// Service used to generate JWT access tokens.
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
        IJwtTokenService jwtTokenService,
        IRegistrationRepository registrationRepository,
        IEmailConfirmationTokenRepository emailConfirmationTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IRevokedJwtTokenRepository revokedJwtTokenRepository,
        IEmailSender emailSender,
        IOptions<ApplicationUrlSettings> applicationUrlOptions)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _secureTokenService = secureTokenService;
        _jwtTokenService = jwtTokenService;
        _registrationRepository = registrationRepository;
        _emailConfirmationTokenRepository =
            emailConfirmationTokenRepository;
        _passwordResetTokenRepository =
            passwordResetTokenRepository;
        _revokedJwtTokenRepository =
            revokedJwtTokenRepository;
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

    /// <summary>
    /// Authenticates a user using email and password credentials.
    /// </summary>
    /// <param name="request">
    /// The credentials provided by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The authenticated user's information and JWT access token.
    /// </returns>
    /// <exception cref="InvalidCredentialsException">
    /// Thrown when the email or password is invalid.
    /// </exception>
    /// <exception cref="EmailNotConfirmedException">
    /// Thrown when the user's email address has not yet been confirmed.
    /// </exception>
    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        // Normalizes the email address before querying the database.
        var email = request.Email
            .Trim()
            .ToLowerInvariant();

        // Retrieves the user associated with the normalized email address.
        var user = await _userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        // Uses the same generic error for unknown users and invalid passwords.
        if (user is null ||
            string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !_passwordHasher.Verify(
                user.PasswordHash,
                request.Password))
        {
            throw new InvalidCredentialsException(
                "Invalid email or password.");
        }

        // Prevents login until the user has confirmed the email address.
        if (!user.EmailConfirmed)
        {
            throw new EmailNotConfirmedException(
                "Please confirm your email address before signing in.");
        }

        // Resolves the role name that will be included in the JWT.
        var role = ResolveRoleName(
            user.RoleId);

        // Generates the signed JWT access token.
        var tokenResult = _jwtTokenService.GenerateToken(
            user.UserId,
            user.Email,
            role);

        return new LoginResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = role,
            AccessToken = tokenResult.AccessToken,
            ExpiresAt = tokenResult.ExpiresAt
        };
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(
    ForgotPasswordRequest request,
    CancellationToken cancellationToken = default)
    {
        const string genericMessage =
            "If an account exists for this email, a password reset link has been sent.";

        var email = request.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        // Always return the same response to prevent email enumeration.
        if (user is null)
        {
            return new ForgotPasswordResponse
            {
                Message = genericMessage
            };
        }

        var now = DateTime.UtcNow;

        // Invalidates previous unused password reset tokens.
        var previousTokens =
            await _passwordResetTokenRepository.GetUnusedByUserIdAsync(
                user.UserId,
                cancellationToken);

        foreach (var previousToken in previousTokens)
        {
            previousToken.UsedAt = now;
        }

        // Generates a cryptographically secure reset token.
        var rawToken = _secureTokenService.GenerateToken();

        // Only the token hash is stored in the database.
        var tokenHash = _secureTokenService.HashToken(rawToken);

        var passwordResetToken = new PasswordResetToken
        {
            UserId = user.UserId,
            TokenHash = tokenHash,
            ExpiresAt = now.Add(PasswordResetTokenLifetime),
            UsedAt = null,
            CreatedAt = now
        };

        await _passwordResetTokenRepository.AddAsync(
            passwordResetToken,
            cancellationToken);

        await _passwordResetTokenRepository.SaveChangesAsync(
            cancellationToken);

        var webBaseUrl =
            _applicationUrls.WebBaseUrl.TrimEnd('/');

        var resetUrl =
            $"{webBaseUrl}/account/reset-password?token={Uri.EscapeDataString(rawToken)}";

        var emailSubject =
             "Reset your DriveFleet password";

        var emailBody = $"""
             <h2>Password reset</h2>

             <p>Hello {user.FirstName},</p>

             <p>
                We received a request to reset the password
                for your DriveFleet account.
             </p>

             <p>
                <a href="{resetUrl}">
                    Reset your password
                </a>
             </p>

            <p>
                This link is valid for 30 minutes.
            </p>

            <p>
                If you did not request a password reset,
                you can ignore this email.
            </p>

            <p>
                DriveFleet
            </p>
            """;

            await _emailSender.SendAsync(
            user.Email,
            emailSubject,
            emailBody,
            cancellationToken);

        return new ForgotPasswordResponse
        {
            Message = genericMessage
        };
    }

    /// <summary>
    /// Resets a user's password using a valid password reset token.
    /// </summary>
    /// <param name="request">
    /// The reset token and new password information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <exception cref="InvalidTokenException">
    /// Thrown when the reset token is invalid, expired or already used.
    /// </exception>
    public async Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        // Rejects missing reset tokens.
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new InvalidTokenException(
                "The password reset token is invalid.");
        }

        // Hashes the raw token so it can be compared
        // with the hash stored in the database.
        var tokenHash =
            _secureTokenService.HashToken(
                request.Token.Trim());

        var passwordResetToken =
            await _passwordResetTokenRepository
                .GetByTokenHashAsync(
                    tokenHash,
                    cancellationToken);

        // Rejects tokens that do not exist.
        if (passwordResetToken is null)
        {
            throw new InvalidTokenException(
                "The password reset token is invalid.");
        }

        // Rejects tokens that have already been used.
        if (passwordResetToken.UsedAt.HasValue)
        {
            throw new InvalidTokenException(
                "The password reset token has already been used.");
        }

        var now = DateTime.UtcNow;

        // Rejects expired password reset tokens.
        if (passwordResetToken.ExpiresAt <= now)
        {
            throw new InvalidTokenException(
                "The password reset token has expired.");
        }

        // Retrieves the user associated with the reset token.
        var user = await _userRepository.GetByIdAsync(
            passwordResetToken.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new InvalidTokenException(
                "The password reset token is invalid.");
        }

        // Generates a secure hash for the new password.
        user.PasswordHash =
            _passwordHasher.Hash(
                request.NewPassword);

        user.UpdatedAt = now;

        // Invalidates every outstanding reset token for this user.
        var unusedTokens =
            await _passwordResetTokenRepository
                .GetUnusedByUserIdAsync(
                    user.UserId,
                    cancellationToken);

        foreach (var unusedToken in unusedTokens)
        {
            unusedToken.UsedAt = now;
        }

        // Persists the new password and token invalidation.
        await _passwordResetTokenRepository
            .SaveChangesAsync(
                cancellationToken);
    }

    /// <summary>
    /// Resolves a role identifier to its corresponding role name.
    /// </summary>
    /// <param name="roleId">
    /// The role identifier stored for the user.
    /// </param>
    /// <returns>
    /// The corresponding DriveFleet role name.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the user has an unsupported role identifier.
    /// </exception>
    private static string ResolveRoleName(
        int roleId)
    {
        return roleId switch
        {
            RoleIds.Admin => RoleNames.Admin,
            RoleIds.Employee => RoleNames.Employee,
            RoleIds.Client => RoleNames.Client,

            _ => throw new InvalidOperationException(
                "The user has an invalid role.")
        };
    }

    /// <summary>
    /// Changes the password of an authenticated user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the authenticated user.
    /// </param>
    /// <param name="request">
    /// The current password and new password information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <exception cref="InvalidCurrentPasswordException">
    /// Thrown when the current password is incorrect.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the authenticated user cannot be found.
    /// </exception>
    public async Task ChangePasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "The authenticated user could not be found.");
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new InvalidCurrentPasswordException(
                "The current password is invalid.");
        }

        var currentPasswordIsValid =
            _passwordHasher.Verify(
                user.PasswordHash,
                request.CurrentPassword);

        if (!currentPasswordIsValid)
        {
            throw new InvalidCurrentPasswordException(
                "The current password is invalid.");
        }

        user.PasswordHash =
            _passwordHasher.Hash(
                request.NewPassword);

        user.UpdatedAt = DateTime.UtcNow;

        await _passwordResetTokenRepository
            .SaveChangesAsync(
                cancellationToken);
    }

    /// <summary>
    /// Revokes a JWT access token before its normal expiration time.
    /// </summary>
    /// <param name="jti">
    /// The unique identifier stored in the JWT jti claim.
    /// </param>
    /// <param name="expiresAt">
    /// The UTC date and time when the JWT naturally expires.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    public async Task RevokeTokenAsync(
        string jti,
        DateTime expiresAt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            throw new ArgumentException(
                "The JWT identifier is required.",
                nameof(jti));
        }

        var normalizedJti = jti.Trim();

        var alreadyRevoked =
            await _revokedJwtTokenRepository.IsRevokedAsync(
                normalizedJti,
                cancellationToken);

        if (alreadyRevoked)
        {
            return;
        }

        var revokedToken = new RevokedJwtToken
        {
            Jti = normalizedJti,
            ExpiresAt = expiresAt,
            RevokedAt = DateTime.UtcNow
        };

        await _revokedJwtTokenRepository.AddAsync(
            revokedToken,
            cancellationToken);
    }
}
