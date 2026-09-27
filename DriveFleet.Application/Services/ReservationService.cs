using DriveFleet.Application.DTOs.Reservations;
using DriveFleet.Application.Exceptions;
using DriveFleet.Application.Filters;
using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Constants;
using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Services;

/// <summary>
/// Provides application operations for reservation management.
/// </summary>
public class ReservationService : IReservationService
{
    private const string UnavailableVehicleStatusName =
        "Unavailable";

    private readonly IReservationRepository _reservationRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IExtraRepository _extraRepository;
    private readonly IEmailSender _emailSender;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReservationService"/> class.
    /// </summary>
    /// <param name="reservationRepository">
    /// Repository used to access and persist reservation data.
    /// </param>
    /// <param name="vehicleRepository">
    /// Repository used to access vehicle data.
    /// </param>
    /// <param name="extraRepository">
    /// Repository used to access reservation extras.
    /// </param>
    public ReservationService(
        IReservationRepository reservationRepository,
        IVehicleRepository vehicleRepository,
        IExtraRepository extraRepository ,
        IEmailSender emailSender)
    {
        _reservationRepository = reservationRepository;
        _vehicleRepository = vehicleRepository;
        _extraRepository = extraRepository;
        _emailSender = emailSender;
    }

    /// <inheritdoc />
    public async Task<ReservationResponse> CreateAsync(
        int userId,
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var extrasById =
            await LoadRequestedExtrasAsync(
                request,
                cancellationToken);

        var reservation = new Reservation
        {
            UserId = userId,
            ReservationStatusId = ReservationStatusIds.Pending,
            TotalValue = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        decimal reservationTotal = 0;

        foreach (var vehicleRequest in request.Vehicles)
        {
            var reservationVehicle =
                await CreateReservationVehicleAsync(
                    vehicleRequest,
                    extrasById,
                    cancellationToken);

            reservation.ReservationVehicles.Add(
                reservationVehicle);

            reservationTotal +=
                CalculateVehicleTotal(reservationVehicle);
        }

        reservation.TotalValue = reservationTotal;

        await _reservationRepository.AddAsync(
            reservation,
            cancellationToken);

        await _reservationRepository.SaveChangesAsync(
            cancellationToken);

        var createdReservation =
            await _reservationRepository.GetByIdAsync(
                reservation.ReservationId,
                cancellationToken);

        if (createdReservation is null)
        {
            throw new InvalidOperationException(
                "The created reservation could not be retrieved.");
        }
        await SendReservationConfirmationEmailAsync(
            createdReservation,
            cancellationToken);

        return MapToResponse(createdReservation);
    }

    /// <inheritdoc />
    public async Task<ReservationResponse?> GetByIdAsync(
        int reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation =
            await _reservationRepository.GetByIdAsync(
                reservationId,
                cancellationToken);

        return reservation is null
            ? null
            : MapToResponse(reservation);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReservationResponse>>
        GetByUserIdAsync(
            int userId,
            ReservationFilterRequest filterRequest,
            CancellationToken cancellationToken = default)
    {
        var filter =
            CreateReservationFilter(
                filterRequest);

        var reservations =
            await _reservationRepository.GetByUserIdAsync(
                userId,
                filter,
                cancellationToken);

        return reservations
            .Select(MapToResponse)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReservationResponse>>
        GetAllAsync(
            ReservationFilterRequest filterRequest,
            CancellationToken cancellationToken = default)
    {
        var filter =
            CreateReservationFilter(
                filterRequest);

        var reservations =
            await _reservationRepository.GetAllAsync(
                filter,
                cancellationToken);

        return reservations
            .Select(MapToResponse)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<bool> StartAsync(
        int reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation =
            await _reservationRepository.GetByIdAsync(
                reservationId,
                cancellationToken);

        if (reservation is null)
        {
            return false;
        }

        await ApplyStatusTransitionAsync(
            reservation,
            ReservationStatusIds.Pending,
            ReservationStatusIds.Active,
            "Only pending reservations can be started.",
            cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> CompleteAsync(
        int reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation =
            await _reservationRepository.GetByIdAsync(
                reservationId,
                cancellationToken);

        if (reservation is null)
        {
            return false;
        }

        await ApplyStatusTransitionAsync(
            reservation,
            ReservationStatusIds.Active,
            ReservationStatusIds.Completed,
            "Only active reservations can be completed.",
            cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> CancelAsync(
        int reservationId,
        int userId,
        bool canManageAllReservations,
        CancellationToken cancellationToken = default)
    {
        var reservation =
            await _reservationRepository.GetByIdAsync(
                reservationId,
                cancellationToken);

        if (reservation is null)
        {
            return false;
        }

        if (!canManageAllReservations &&
            reservation.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You cannot cancel another user's reservation.");
        }
        if (!canManageAllReservations)
        {
            ValidateClientCancellationPeriod(
                reservation);
        }

        await ApplyStatusTransitionAsync(
            reservation,
            ReservationStatusIds.Pending,
            ReservationStatusIds.Cancelled,
            "Only pending reservations can be cancelled.",
            cancellationToken);

        if (!canManageAllReservations)
        {
            await SendCancellationEmailAsync(
                reservation,
                cancellationToken);
        }
        return true;
    }

    /// <summary>
    /// Sends a confirmation email after a reservation
    /// has been created successfully.
    /// </summary>
    /// <param name="reservation">
    /// The created reservation.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    private async Task SendReservationConfirmationEmailAsync(
        Reservation reservation,
        CancellationToken cancellationToken)
    {
        var vehicleDetails =
            string.Join(
                "",
                reservation.ReservationVehicles.Select(
                    reservationVehicle =>
                    {
                        var extras =
                            reservationVehicle.ReservationVehicleExtras.Count == 0
                                ? "No extras selected."
                                : string.Join(
                                    ", ",
                                    reservationVehicle
                                        .ReservationVehicleExtras
                                        .Select(reservationVehicleExtra =>
                                            $"{reservationVehicleExtra.Extra.Name} " +
                                            $"x{reservationVehicleExtra.Quantity}"));

                        return $"""
                        <li>
                            <strong>
                                {reservationVehicle.Vehicle.Brand}
                                {reservationVehicle.Vehicle.Model}
                            </strong>
                            <br />
                            Start:
                            {reservationVehicle.StartDate:dd/MM/yyyy HH:mm}
                            <br />
                            End:
                            {reservationVehicle.EndDate:dd/MM/yyyy HH:mm}
                            <br />
                            Extras:
                            {extras}
                        </li>
                        """;
                    }));

        var subject =
            $"DriveFleet reservation #{reservation.ReservationId} confirmed";

        var body = $"""
        <h2>Reservation confirmed</h2>

        <p>
            Hello {reservation.User.FirstName},
        </p>

        <p>
            Your DriveFleet reservation
            <strong>#{reservation.ReservationId}</strong>
            has been created successfully.
        </p>

        <p>
            <strong>Status:</strong>
            Pending
        </p>

        <h3>Vehicles</h3>

        <ul>
            {vehicleDetails}
        </ul>

        <p>
            <strong>Total:</strong>
            {reservation.TotalValue:C}
        </p>

        <p>
            Thank you for choosing DriveFleet.
        </p>
        """;

        await _emailSender.SendAsync(
            reservation.User.Email,
            subject,
            body,
            cancellationToken);
    }
    /// <summary>
    /// Sends the reservation cancellation confirmation email
    /// to the reservation owner.
    /// </summary>
    /// <param name="reservation">
    /// The cancelled reservation.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    private async Task SendCancellationEmailAsync(
        Reservation reservation,
        CancellationToken cancellationToken)
    {
        var vehicleNames =
            string.Join(
                ", ",
                reservation.ReservationVehicles
                    .Select(reservationVehicle =>
                        $"{reservationVehicle.Vehicle.Brand} " +
                        $"{reservationVehicle.Vehicle.Model}"));

        var subject =
            $"DriveFleet reservation #{reservation.ReservationId} cancelled";

        var body = $"""
        <h2>Reservation cancelled</h2>

        <p>Hello {reservation.User.FirstName},</p>

        <p>
            Your DriveFleet reservation
            <strong>#{reservation.ReservationId}</strong>
            has been cancelled successfully.
        </p>

        <p>
            <strong>Vehicles:</strong>
            {vehicleNames}
        </p>

        <p>
            <strong>Total:</strong>
            {reservation.TotalValue:C}
        </p>

        <p>
            Thank you for using DriveFleet.
        </p>
        """;

        await _emailSender.SendAsync(
            reservation.User.Email,
            subject,
            body,
            cancellationToken);
    }

    /// <summary>
    /// Validates whether a client may still cancel
    /// the specified reservation.
    /// </summary>
    /// <param name="reservation">
    /// The reservation being cancelled.
    /// </param>
    /// <exception cref="ConflictException">
    /// Thrown when the rental period has already started.
    /// </exception>
    private static void ValidateClientCancellationPeriod(
        Reservation reservation)
    {
        var firstStartDate =
            reservation.ReservationVehicles
                .Min(vehicle => vehicle.StartDate);

        if (firstStartDate <= DateTime.UtcNow)
        {
            throw new ConflictException(
                "The reservation can no longer be cancelled because the rental period has already started.");
        }
    }

    /// <summary>
    /// Applies a validated status transition to a reservation.
    /// </summary>
    /// <param name="reservation">
    /// The reservation being updated.
    /// </param>
    /// <param name="expectedStatusId">
    /// The status required before the transition.
    /// </param>
    /// <param name="targetStatusId">
    /// The status assigned after the transition.
    /// </param>
    /// <param name="conflictMessage">
    /// The message returned when the transition is not allowed.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="ConflictException">
    /// Thrown when the reservation cannot transition
    /// from its current status.
    /// </exception>
    private async Task ApplyStatusTransitionAsync(
        Reservation reservation,
        int expectedStatusId,
        int targetStatusId,
        string conflictMessage,
        CancellationToken cancellationToken)
    {

        if (reservation.ReservationStatusId !=
            expectedStatusId)
        {
            throw new ConflictException(
                conflictMessage);
        }

        reservation.ReservationStatusId =
            targetStatusId;

        reservation.UpdatedAt =
            DateTime.UtcNow;

        await _reservationRepository.SaveChangesAsync(
            cancellationToken);
    }

    /// <summary>
    /// Creates validated reservation filtering criteria
    /// from the supplied request.
    /// </summary>
    /// <param name="filterRequest">
    /// The reservation filter request.
    /// </param>
    /// <returns>
    /// The validated reservation filter.
    /// </returns>
    /// <exception cref="InvalidReservationFilterException">
    /// Thrown when the filtering criteria are invalid.
    /// </exception>
    private static ReservationFilter CreateReservationFilter(
        ReservationFilterRequest filterRequest)
    {
        ValidateReservationFilter(
            filterRequest);

        return new ReservationFilter
        {
            StatusId =
                filterRequest.StatusId,

            FromDate =
                filterRequest.FromDate?.Date,

            ToDate =
                filterRequest.ToDate?.Date
        };
    }

    /// <summary>
    /// Validates reservation filtering criteria.
    /// </summary>
    /// <param name="filterRequest">
    /// The reservation filter request to validate.
    /// </param>
    /// <exception cref="InvalidReservationFilterException">
    /// Thrown when the status or date range is invalid.
    /// </exception>
    private static void ValidateReservationFilter(
        ReservationFilterRequest filterRequest)
    {
        if (filterRequest.StatusId.HasValue &&
            !IsValidReservationStatusId(
                filterRequest.StatusId.Value))
        {
            throw new InvalidReservationFilterException(
                "The selected reservation status is invalid.");
        }

        if (filterRequest.FromDate.HasValue &&
            filterRequest.ToDate.HasValue &&
            filterRequest.FromDate.Value.Date >
            filterRequest.ToDate.Value.Date)
        {
            throw new InvalidReservationFilterException(
                "The start date cannot be later than the end date.");
        }
    }

    /// <summary>
    /// Determines whether a reservation status identifier
    /// is supported by the application.
    /// </summary>
    /// <param name="statusId">
    /// The reservation status identifier.
    /// </param>
    /// <returns>
    /// True when the status identifier is valid;
    /// otherwise, false.
    /// </returns>
    private static bool IsValidReservationStatusId(
        int statusId)
    {
        return statusId ==
                   ReservationStatusIds.Pending ||
               statusId ==
                   ReservationStatusIds.Active ||
               statusId ==
                   ReservationStatusIds.Completed ||
               statusId ==
                   ReservationStatusIds.Cancelled;
    }

    /// <summary>
    /// Validates the structural rules required to create a reservation.
    /// </summary>
    /// <param name="request">The reservation creation request.</param>
    /// <exception cref="InvalidReservationException">
    /// Thrown when the request violates a reservation rule.
    /// </exception>
    private static void ValidateCreateRequest(
        CreateReservationRequest request)
    {
        if (request.Vehicles.Count == 0)
        {
            throw new InvalidReservationException(
                "At least one vehicle must be selected.");
        }

        var duplicatedVehicleIds =
            request.Vehicles
                .GroupBy(vehicle => vehicle.VehicleId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

        if (duplicatedVehicleIds.Count > 0)
        {
            throw new InvalidReservationException(
                "The same vehicle cannot be added more than once.");
        }

        foreach (var vehicle in request.Vehicles)
        {
            if (vehicle.VehicleId <= 0)
            {
                throw new InvalidReservationException(
                    "A valid vehicle must be selected.");
            }

            if (vehicle.EndDate <= vehicle.StartDate)
            {
                throw new InvalidReservationException(
                    "The end date must be later than the start date.");
            }

            if (vehicle.StartDate < DateTime.UtcNow)
            {
                throw new InvalidReservationException(
                    "The reservation start date cannot be in the past.");
            }

            var duplicatedExtraIds =
                vehicle.Extras
                    .GroupBy(extra => extra.ExtraId)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToList();

            if (duplicatedExtraIds.Count > 0)
            {
                throw new InvalidReservationException(
                    "The same extra cannot be added more than once " +
                    "to the same vehicle.");
            }

            if (vehicle.Extras.Any(extra =>
                extra.ExtraId <= 0 ||
                extra.Quantity <= 0))
            {
                throw new InvalidReservationException(
                    "Reservation extras must have a valid identifier " +
                    "and a quantity greater than zero.");
            }
        }
    }

    /// <summary>
    /// Loads all active extras requested across the reservation.
    /// </summary>
    /// <param name="request">The reservation creation request.</param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A dictionary containing the requested extras indexed by identifier.
    /// </returns>
    private async Task<IReadOnlyDictionary<int, Extra>>
        LoadRequestedExtrasAsync(
            CreateReservationRequest request,
            CancellationToken cancellationToken)
    {
        var requestedExtraIds =
            request.Vehicles
                .SelectMany(vehicle => vehicle.Extras)
                .Select(extra => extra.ExtraId)
                .Distinct()
                .ToList();

        if (requestedExtraIds.Count == 0)
        {
            return new Dictionary<int, Extra>();
        }

        var extras =
            await _extraRepository.GetActiveByIdsAsync(
                requestedExtraIds,
                cancellationToken);

        if (extras.Count != requestedExtraIds.Count)
        {
            throw new InvalidReservationException(
                "One or more selected extras do not exist or are inactive.");
        }

        return extras.ToDictionary(
            extra => extra.ExtraId);
    }

    /// <summary>
    /// Creates a reservation vehicle after validating vehicle availability.
    /// </summary>
    /// <param name="request">
    /// The vehicle reservation data.
    /// </param>
    /// <param name="extrasById">
    /// The active extras available for the reservation.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>The reservation vehicle entity.</returns>
    private async Task<ReservationVehicle>
        CreateReservationVehicleAsync(
            CreateReservationVehicleRequest request,
            IReadOnlyDictionary<int, Extra> extrasById,
            CancellationToken cancellationToken)
    {
        var vehicle =
            await _vehicleRepository.GetByIdAsync(
                request.VehicleId,
                cancellationToken);

        if (vehicle is null)
        {
            throw new InvalidReservationException(
                $"Vehicle {request.VehicleId} does not exist.");
        }

        if (string.Equals(
            vehicle.VehicleStatus.Name,
            UnavailableVehicleStatusName,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                $"Vehicle {request.VehicleId} is unavailable.");
        }

        var hasOverlap =
            await _reservationRepository.HasVehicleOverlapAsync(
                request.VehicleId,
                request.StartDate,
                request.EndDate,
                cancellationToken);

        if (hasOverlap)
        {
            throw new ConflictException(
                $"Vehicle {request.VehicleId} is already reserved " +
                "for the selected period.");
        }

        var reservationVehicle =
            new ReservationVehicle
            {
                VehicleId = vehicle.VehicleId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DailyPrice = vehicle.DailyPrice
            };

        foreach (var extraRequest in request.Extras)
        {
            var extra =
                extrasById[extraRequest.ExtraId];

            reservationVehicle.ReservationVehicleExtras.Add(
                new ReservationVehicleExtra
                {
                    ExtraId = extra.ExtraId,
                    Quantity = extraRequest.Quantity,
                    Price = extra.Price
                });
        }

        return reservationVehicle;
    }

    /// <summary>
    /// Calculates the total value of a reserved vehicle,
    /// including its selected extras.
    /// </summary>
    /// <param name="reservationVehicle">
    /// The reserved vehicle to calculate.
    /// </param>
    /// <returns>The total value of the reserved vehicle.</returns>
    private static decimal CalculateVehicleTotal(
        ReservationVehicle reservationVehicle)
    {
        var rentalDays =
            CalculateRentalDays(
                reservationVehicle.StartDate,
                reservationVehicle.EndDate);

        var rentalValue =
            rentalDays * reservationVehicle.DailyPrice;

        var extrasValue =
            reservationVehicle.ReservationVehicleExtras
                .Sum(extra => extra.Quantity * extra.Price);

        return rentalValue + extrasValue;
    }

    /// <summary>
    /// Calculates the number of billable rental days.
    /// Partial days are charged as full days.
    /// </summary>
    /// <param name="startDate">The rental start date.</param>
    /// <param name="endDate">The rental end date.</param>
    /// <returns>The number of billable rental days.</returns>
    private static int CalculateRentalDays(
        DateTime startDate,
        DateTime endDate)
    {
        return Math.Max(
            1,
            (int)Math.Ceiling(
                (endDate - startDate).TotalDays));
    }

    /// <summary>
    /// Maps a reservation entity to the response returned to clients.
    /// </summary>
    /// <param name="reservation">The reservation entity.</param>
    /// <returns>The mapped reservation response.</returns>
    private static ReservationResponse MapToResponse(
        Reservation reservation)
    {
        return new ReservationResponse
        {
            ReservationId = reservation.ReservationId,
            UserId = reservation.UserId,
            UserFullName =
                $"{reservation.User.FirstName} " +
                $"{reservation.User.LastName}",
            UserEmail = reservation.User.Email,
            ReservationStatusId =
                reservation.ReservationStatusId,
            ReservationStatusName =
                reservation.ReservationStatus.Name,
            TotalValue = reservation.TotalValue,
            CreatedAt = reservation.CreatedAt,
            UpdatedAt = reservation.UpdatedAt,
            Vehicles = reservation.ReservationVehicles
                .OrderBy(vehicle => vehicle.StartDate)
                .Select(MapVehicleToResponse)
                .ToList()
        };
    }

    /// <summary>
    /// Maps a reservation vehicle entity to its response model.
    /// </summary>
    /// <param name="reservationVehicle">
    /// The reservation vehicle entity.
    /// </param>
    /// <returns>The mapped reservation vehicle response.</returns>
    private static ReservationVehicleResponse MapVehicleToResponse(
        ReservationVehicle reservationVehicle)
    {
        var rentalDays =
            CalculateRentalDays(
                reservationVehicle.StartDate,
                reservationVehicle.EndDate);

        var rentalValue =
            rentalDays * reservationVehicle.DailyPrice;

        var extras =
            reservationVehicle.ReservationVehicleExtras
                .Select(MapExtraToResponse)
                .ToList();

        var extrasValue =
            extras.Sum(extra => extra.Total);

        return new ReservationVehicleResponse
        {
            ReservationVehicleId =
                reservationVehicle.ReservationVehicleId,
            VehicleId = reservationVehicle.VehicleId,
            VehicleBrand =
                reservationVehicle.Vehicle.Brand,
            VehicleModel =
                reservationVehicle.Vehicle.Model,
            LicensePlate =
                reservationVehicle.Vehicle.LicensePlate,
            StartDate = reservationVehicle.StartDate,
            EndDate = reservationVehicle.EndDate,
            DailyPrice = reservationVehicle.DailyPrice,
            RentalDays = rentalDays,
            RentalValue = rentalValue,
            ExtrasValue = extrasValue,
            TotalValue = rentalValue + extrasValue,
            Extras = extras
        };
    }

    /// <summary>
    /// Maps a reservation extra entity to its response model.
    /// </summary>
    /// <param name="reservationExtra">
    /// The reservation extra entity.
    /// </param>
    /// <returns>The mapped reservation extra response.</returns>
    private static ReservationExtraResponse MapExtraToResponse(
        ReservationVehicleExtra reservationExtra)
    {
        return new ReservationExtraResponse
        {
            ExtraId = reservationExtra.ExtraId,
            Name = reservationExtra.Extra.Name,
            Quantity = reservationExtra.Quantity,
            Price = reservationExtra.Price
        };
    }
}
