using DriveFleet.Application.DTOs.Profile;
using DriveFleet.Application.Interfaces;

namespace DriveFleet.Application.Services;

/// <summary>
/// Provides application operations related to
/// the authenticated user's profile.
/// </summary>
public class ProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IProfilePhotoStorage _profilePhotoStorage;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ProfileService"/> class.
    /// </summary>
    /// <param name="userRepository">
    /// Repository used to retrieve user information.
    /// </param>
    public ProfileService(
    IUserRepository userRepository,
    IProfilePhotoStorage profilePhotoStorage)
    {
        _userRepository = userRepository;
        _profilePhotoStorage = profilePhotoStorage;
    }

    /// <summary>
    /// Retrieves the profile of the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the authenticated user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The user's profile, or null if the user cannot be found.
    /// </returns>
    public async Task<UserProfileResponse?> GetProfileAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new UserProfileResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Photo = user.Photo,
            Provider = user.Provider,
            EmailConfirmed = user.EmailConfirmed,
            Role = user.Role.Name
        };
    }

    /// <summary>
    /// Updates the profile information of the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the authenticated user.
    /// </param>
    /// <param name="request">
    /// The profile information to update.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// True if the user was found and updated; otherwise, false.
    /// </returns>
    public async Task<bool> UpdateProfileAsync(
        int userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            return false;
        }

        user.FirstName = request.FirstName.Trim();

        user.LastName =
            string.IsNullOrWhiteSpace(request.LastName)
                ? null
                : request.LastName.Trim();

        user.Phone =
            string.IsNullOrWhiteSpace(request.Phone)
                ? null
                : request.Phone.Trim();

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    /// <summary>
    /// Updates the profile photo of the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the authenticated user.
    /// </param>
    /// <param name="photoStream">
    /// The stream containing the new profile photo.
    /// </param>
    /// <param name="fileExtension">
    /// The file extension of the uploaded photo.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The relative path of the new profile photo,
    /// or null if the user cannot be found.
    /// </returns>
    public async Task<string?> UpdateProfilePhotoAsync(
        int userId,
        Stream photoStream,
        string fileExtension,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            return null;
        }

        var previousPhoto = user.Photo;

        var newPhotoPath =
            await _profilePhotoStorage.SaveAsync(
                userId,
                photoStream,
                fileExtension,
                cancellationToken);

        user.Photo = newPhotoPath;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync(
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousPhoto))
        {
            await _profilePhotoStorage.DeleteAsync(
                previousPhoto,
                cancellationToken);
        }

        return newPhotoPath;
    }
}
