using DriveFleet.Application.DTOs.Reviews;
using DriveFleet.Application.Exceptions;
using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Constants;
using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Services;

/// <summary>
/// Provides application operations for vehicle reviews.
/// </summary>
public class ReviewService : IReviewService
{
    private const int MinimumStars = 1;
    private const int MaximumStars = 5;
    private const int MaximumCommentLength = 1000;

    private readonly IReviewRepository _reviewRepository;
    private readonly IReservationRepository _reservationRepository;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReviewService"/> class.
    /// </summary>
    public ReviewService(
        IReviewRepository reviewRepository,
        IReservationRepository reservationRepository)
    {
        _reviewRepository = reviewRepository;
        _reservationRepository = reservationRepository;
    }

    /// <inheritdoc />
    public async Task<ReviewResponse?> CreateAsync(
        int userId,
        int reservationId,
        int reservationVehicleId,
        CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(
            request);

        var reservation =
            await _reservationRepository.GetByIdAsync(
                reservationId,
                cancellationToken);

        if (reservation is null)
        {
            return null;
        }

        if (reservation.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You cannot review another user's reservation.");
        }

        if (reservation.ReservationStatusId !=
            ReservationStatusIds.Completed)
        {
            throw new ConflictException(
                "Only completed reservations can be reviewed.");
        }

        var reservationVehicle =
            reservation.ReservationVehicles
                .FirstOrDefault(vehicle =>
                    vehicle.ReservationVehicleId ==
                    reservationVehicleId);

        if (reservationVehicle is null)
        {
            return null;
        }

        var existingReview =
            await _reviewRepository
                .GetByReservationVehicleIdAsync(
                    reservationVehicleId,
                    cancellationToken);

        if (existingReview is not null)
        {
            throw new ConflictException(
                "This vehicle has already been reviewed for this reservation.");
        }

        var comment =
            string.IsNullOrWhiteSpace(
                request.Comment)
                ? null
                : request.Comment.Trim();

        var review =
            new Review
            {
                ReservationVehicleId =
                    reservationVehicleId,

                Stars =
                    (byte)request.Stars,

                Comment =
                    comment,

                IsVisible =
                    true,

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    null
            };

        await _reviewRepository.AddAsync(
            review,
            cancellationToken);

        await _reviewRepository.SaveChangesAsync(
            cancellationToken);

        return new ReviewResponse
        {
            ReviewId =
                review.ReviewId,

            ReservationVehicleId =
                review.ReservationVehicleId,

            VehicleId =
                reservationVehicle.VehicleId,

            Stars =
                review.Stars,

            Comment =
                review.Comment,

            IsVisible =
                review.IsVisible,

            CreatedAt =
                review.CreatedAt
        };
    }

    /// <summary>
    /// Validates review information before persistence.
    /// </summary>
    private static void ValidateRequest(
        CreateReviewRequest request)
    {
        if (request.Stars <
            MinimumStars ||
            request.Stars >
            MaximumStars)
        {
            throw new InvalidReviewException(
                "The rating must be between 1 and 5 stars.");
        }

        if (request.Comment is not null &&
            request.Comment.Trim().Length >
            MaximumCommentLength)
        {
            throw new InvalidReviewException(
                "The review comment cannot exceed 1000 characters.");
        }
    }
}