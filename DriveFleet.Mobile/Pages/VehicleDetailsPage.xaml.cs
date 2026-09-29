using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using DriveFleet.Mobile.Models.Vehicles;
using DriveFleet.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace DriveFleet.Mobile.Pages;

/// <summary>
/// Displays detailed operational information
/// for a single DriveFleet vehicle.
/// </summary>
public partial class VehicleDetailsPage :
    ContentPage,
    IQueryAttributable
{
    private int _vehicleId;

    private bool _isLoading;

    private bool _isUpdatingStatus;

    private bool _isLoadingHistory;

    private VehicleModel? _currentVehicle;

    /// <summary>
    /// Gets the reservation history displayed
    /// for the current vehicle.
    /// </summary>
    public ObservableCollection<VehicleReservationHistoryItemModel>
        ReservationHistory
    { get; } =
            new();

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VehicleDetailsPage"/> class.
    /// </summary>
    public VehicleDetailsPage()
    {
        InitializeComponent();

        BindingContext =
            this;
    }

    /// <summary>
    /// Receives the selected vehicle identifier
    /// from Shell navigation.
    /// </summary>
    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (!query.TryGetValue(
                "vehicleId",
                out var vehicleIdValue))
        {
            return;
        }

        if (!int.TryParse(
                vehicleIdValue?.ToString(),
                out var vehicleId))
        {
            return;
        }

        _vehicleId =
            vehicleId;
    }

    /// <summary>
    /// Refreshes the selected vehicle whenever
    /// the page becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_vehicleId <= 0)
        {
            ShowError(
                "A valid vehicle was not selected.");

            return;
        }

        await LoadVehicleAsync();
    }

    /// <summary>
    /// Retrieves the selected vehicle
    /// from the DriveFleet API.
    /// </summary>
    private async Task LoadVehicleAsync()
    {
        if (_isLoading)
        {
            return;
        }

        var services =
            IPlatformApplication
                .Current?
                .Services;

        var vehicleApiService =
            services?
                .GetService<VehicleApiService>();

        if (vehicleApiService is null)
        {
            ShowError(
                "The vehicle service is unavailable.");

            return;
        }

        try
        {
            _isLoading =
                true;

            ShowLoading();

            var result =
                await vehicleApiService.GetByIdAsync(
                    _vehicleId);

            if (result.StatusCode ==
                    HttpStatusCode.NotFound ||
                result.Vehicle is null)
            {
                ShowError(
                    "The selected vehicle could not be found.");

                return;
            }

            if (result.StatusCode !=
                HttpStatusCode.OK)
            {
                ShowError(
                    string.IsNullOrWhiteSpace(
                        result.Detail)
                        ? "The vehicle could not be loaded."
                        : result.Detail);

                return;
            }

            PopulateVehicle(
                result.Vehicle,
                vehicleApiService);

            await LoadReservationHistoryAsync();
        }
        catch (HttpRequestException)
        {
            ShowError(
                "Unable to connect to the DriveFleet API.");
        }
        catch (TaskCanceledException)
        {
            ShowError(
                "The vehicle request timed out. Please try again.");
        }
        finally
        {
            _isLoading =
                false;

            LoadingContainer.IsVisible =
                false;
        }
    }

    /// <summary>
    /// Loads all reservations associated
    /// with the current vehicle.
    /// </summary>
    private async Task LoadReservationHistoryAsync()
    {
        if (_isLoadingHistory ||
            _vehicleId <= 0)
        {
            return;
        }

        var services =
            IPlatformApplication
                .Current?
                .Services;

        var sessionService =
            services?
                .GetService<SessionService>();

        var reservationApiService =
            services?
                .GetService<ReservationApiService>();

        if (sessionService is null ||
            reservationApiService is null)
        {
            ShowHistoryError(
                "The reservation history service is unavailable.");

            return;
        }

        var session =
            await sessionService.GetAsync();

        if (session is null)
        {
            await Shell.Current.GoToAsync(
                "//LoginPage");

            return;
        }

        try
        {
            _isLoadingHistory =
                true;

            ShowHistoryLoading();

            var result =
                await reservationApiService.GetAllAsync(
                    session.AccessToken);

            if (result.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                sessionService.Clear();

                await Shell.Current.GoToAsync(
                    "//LoginPage");

                return;
            }

            if (result.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                ShowHistoryError(
                    "You are not authorized to view reservation history.");

                return;
            }

            if (result.StatusCode !=
                HttpStatusCode.OK)
            {
                ShowHistoryError(
                    string.IsNullOrWhiteSpace(
                        result.Detail)
                        ? "The reservation history could not be loaded."
                        : result.Detail);

                return;
            }

            var history =
                result.Reservations
                    .SelectMany(
                        reservation =>
                            reservation.Vehicles
                                .Where(vehicle =>
                                    vehicle.VehicleId ==
                                    _vehicleId)
                                .Select(vehicle =>
                                    new VehicleReservationHistoryItemModel
                                    {
                                        ReservationId =
                                            reservation.ReservationId,

                                        CustomerName =
                                            reservation.UserFullName,

                                        ReservationStatusName =
                                            reservation.ReservationStatusName,

                                        StartDate =
                                            vehicle.StartDate,

                                        EndDate =
                                            vehicle.EndDate,

                                        VehicleTotalValue =
                                            vehicle.TotalValue
                                    }))
                    .OrderByDescending(item =>
                        item.StartDate)
                    .ThenByDescending(item =>
                        item.ReservationId)
                    .ToList();

            ReservationHistory.Clear();

            foreach (var item in history)
            {
                ReservationHistory.Add(
                    item);
            }

            HistoryErrorContainer.IsVisible =
                false;

            HistoryListContainer.IsVisible =
                ReservationHistory.Count > 0;

            HistoryEmptyContainer.IsVisible =
                ReservationHistory.Count == 0;
        }
        catch (HttpRequestException)
        {
            ShowHistoryError(
                "Unable to connect to the DriveFleet API.");
        }
        catch (TaskCanceledException)
        {
            ShowHistoryError(
                "The reservation history request timed out.");
        }
        finally
        {
            _isLoadingHistory =
                false;

            HistoryLoadingContainer.IsVisible =
                false;
        }
    }

    /// <summary>
    /// Populates the vehicle details interface.
    /// </summary>
    private void PopulateVehicle(
        VehicleModel vehicle,
        VehicleApiService vehicleApiService)
    {
        _currentVehicle =
            vehicle;

        VehicleNameLabel.Text =
            $"{vehicle.Brand} {vehicle.Model}";

        CategoryYearLabel.Text =
            $"{vehicle.CategoryName} · {vehicle.Year}";

        LicensePlateLabel.Text =
            vehicle.LicensePlate;

        SeatsLabel.Text =
            vehicle.Seats.ToString(
                CultureInfo.InvariantCulture);

        YearLabel.Text =
            vehicle.Year.ToString(
                CultureInfo.InvariantCulture);

        CategoryLabel.Text =
            vehicle.CategoryName;

        DailyPriceLabel.Text =
            $"€{vehicle.DailyPrice:F2} / day";

        OperationalStatusLabel.Text =
            vehicle.VehicleStatusName;

        StatusLabel.Text =
            vehicle.VehicleStatusName
                .ToUpperInvariant();

        DescriptionLabel.Text =
            vehicle.Description ??
            string.Empty;

        DescriptionCard.IsVisible =
            !string.IsNullOrWhiteSpace(
                vehicle.Description);

        ApplyStatusAppearance(
            vehicle.VehicleStatusName);

        UpdateStatusButtons(
            vehicle.VehicleStatusId);

        var mainPhoto =
            vehicle.Photos
                .OrderBy(photo =>
                    photo.DisplayOrder)
                .FirstOrDefault(photo =>
                    photo.DisplayOrder == 1)
            ??
            vehicle.Photos
                .OrderBy(photo =>
                    photo.DisplayOrder)
                .FirstOrDefault();

        var imageUrl =
            vehicleApiService
                .GetPublicResourceUrl(
                    mainPhoto?.FilePath);

        MainPhotoImage.Source =
            imageUrl;

        MainPhotoImage.IsVisible =
            !string.IsNullOrWhiteSpace(
                imageUrl);

        ErrorContainer.IsVisible =
            false;

        VehicleContent.IsVisible =
            true;
    }

    /// <summary>
    /// Applies the appropriate status colors.
    /// </summary>
    private void ApplyStatusAppearance(
        string statusName)
    {
        var backgroundResource =
            "DriveFleetAccentSoft";

        var foregroundResource =
            "DriveFleetAccent";

        if (string.Equals(
            statusName,
            "Available",
            StringComparison.OrdinalIgnoreCase))
        {
            backgroundResource =
                "DriveFleetSuccessSoft";

            foregroundResource =
                "DriveFleetSuccess";
        }
        else if (string.Equals(
            statusName,
            "Reserved",
            StringComparison.OrdinalIgnoreCase))
        {
            backgroundResource =
                "DriveFleetWarningSoft";

            foregroundResource =
                "DriveFleetWarning";
        }
        else if (string.Equals(
            statusName,
            "Unavailable",
            StringComparison.OrdinalIgnoreCase))
        {
            backgroundResource =
                "DriveFleetDangerSoft";

            foregroundResource =
                "DriveFleetDanger";
        }

        var backgroundColor =
            GetColorResource(
                backgroundResource);

        var foregroundColor =
            GetColorResource(
                foregroundResource);

        StatusBorder.BackgroundColor =
            backgroundColor;

        StatusLabel.TextColor =
            foregroundColor;

        StatusIndicatorBorder.BackgroundColor =
            backgroundColor;

        StatusIndicatorLabel.TextColor =
            foregroundColor;
    }

    /// <summary>
    /// Marks the current vehicle as available.
    /// </summary>
    private async void OnMarkAvailableClicked(
        object sender,
        EventArgs e)
    {
        await UpdateVehicleStatusAsync(
            VehicleStatusIds.Available,
            "Available");
    }

    /// <summary>
    /// Marks the current vehicle as unavailable.
    /// </summary>
    private async void OnMarkUnavailableClicked(
        object sender,
        EventArgs e)
    {
        await UpdateVehicleStatusAsync(
            VehicleStatusIds.Unavailable,
            "Unavailable");
    }

    /// <summary>
    /// Updates the current vehicle operational status.
    /// </summary>
    private async Task UpdateVehicleStatusAsync(
        int targetStatusId,
        string targetStatusName)
    {
        if (_isUpdatingStatus ||
            _currentVehicle is null)
        {
            return;
        }

        if (_currentVehicle.VehicleStatusId ==
            targetStatusId)
        {
            return;
        }

        var services =
            IPlatformApplication
                .Current?
                .Services;

        var sessionService =
            services?
                .GetService<SessionService>();

        var vehicleApiService =
            services?
                .GetService<VehicleApiService>();

        if (sessionService is null ||
            vehicleApiService is null)
        {
            ShowStatusError(
                "The vehicle status service is unavailable.");

            return;
        }

        var session =
            await sessionService.GetAsync();

        if (session is null)
        {
            await Shell.Current.GoToAsync(
                "//LoginPage");

            return;
        }

        var confirmed =
            await DisplayAlert(
                "Update vehicle status",
                $"Change this vehicle to {targetStatusName}?",
                "Change status",
                "Cancel");

        if (!confirmed)
        {
            return;
        }

        try
        {
            _isUpdatingStatus =
                true;

            SetStatusUpdatingState(
                true);

            HideStatusMessage();

            var result =
                await vehicleApiService.UpdateStatusAsync(
                    session.AccessToken,
                    _currentVehicle,
                    targetStatusId);

            if (result.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                sessionService.Clear();

                await Shell.Current.GoToAsync(
                    "//LoginPage");

                return;
            }

            if (result.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                ShowStatusError(
                    "You are not authorized to update this vehicle.");

                return;
            }

            if (result.StatusCode ==
                HttpStatusCode.NotFound)
            {
                ShowStatusError(
                    "The selected vehicle no longer exists.");

                return;
            }

            if (result.StatusCode ==
                    HttpStatusCode.BadRequest ||
                result.StatusCode ==
                    HttpStatusCode.Conflict)
            {
                ShowStatusError(
                    string.IsNullOrWhiteSpace(
                        result.Detail)
                        ? "The vehicle status could not be updated."
                        : result.Detail);

                return;
            }

            if (result.StatusCode !=
                HttpStatusCode.NoContent)
            {
                ShowStatusError(
                    "The vehicle status could not be updated.");

                return;
            }

            ShowStatusSuccess(
                $"Vehicle status changed to {targetStatusName}.");

            await LoadVehicleAsync();
        }
        catch (HttpRequestException)
        {
            ShowStatusError(
                "Unable to connect to the DriveFleet API.");
        }
        catch (TaskCanceledException)
        {
            ShowStatusError(
                "The status update request timed out.");
        }
        finally
        {
            _isUpdatingStatus =
                false;

            SetStatusUpdatingState(
                false);
        }
    }

    /// <summary>
    /// Updates the availability of status buttons.
    /// </summary>
    private void UpdateStatusButtons(
        int vehicleStatusId)
    {
        MarkAvailableButton.IsEnabled =
            vehicleStatusId !=
            VehicleStatusIds.Available;

        MarkUnavailableButton.IsEnabled =
            vehicleStatusId !=
            VehicleStatusIds.Unavailable;
    }

    /// <summary>
    /// Updates the status controls while
    /// a request is running.
    /// </summary>
    private void SetStatusUpdatingState(
        bool isUpdating)
    {
        if (isUpdating)
        {
            MarkAvailableButton.IsEnabled =
                false;

            MarkUnavailableButton.IsEnabled =
                false;

            return;
        }

        if (_currentVehicle is not null)
        {
            UpdateStatusButtons(
                _currentVehicle.VehicleStatusId);
        }
    }

    /// <summary>
    /// Displays a successful status message.
    /// </summary>
    private void ShowStatusSuccess(
        string message)
    {
        StatusMessageBorder.BackgroundColor =
            GetColorResource(
                "DriveFleetSuccessSoft");

        StatusMessageLabel.TextColor =
            GetColorResource(
                "DriveFleetSuccess");

        StatusMessageLabel.Text =
            message;

        StatusMessageBorder.IsVisible =
            true;
    }

    /// <summary>
    /// Displays a status update error.
    /// </summary>
    private void ShowStatusError(
        string message)
    {
        StatusMessageBorder.BackgroundColor =
            GetColorResource(
                "DriveFleetDangerSoft");

        StatusMessageLabel.TextColor =
            GetColorResource(
                "DriveFleetDanger");

        StatusMessageLabel.Text =
            message;

        StatusMessageBorder.IsVisible =
            true;
    }

    /// <summary>
    /// Clears the current status message.
    /// </summary>
    private void HideStatusMessage()
    {
        StatusMessageLabel.Text =
            string.Empty;

        StatusMessageBorder.IsVisible =
            false;
    }

    /// <summary>
    /// Displays the history loading state.
    /// </summary>
    private void ShowHistoryLoading()
    {
        HistoryLoadingContainer.IsVisible =
            true;

        HistoryErrorContainer.IsVisible =
            false;

        HistoryEmptyContainer.IsVisible =
            false;

        HistoryListContainer.IsVisible =
            false;
    }

    /// <summary>
    /// Displays a reservation history error.
    /// </summary>
    private void ShowHistoryError(
        string message)
    {
        HistoryLoadingContainer.IsVisible =
            false;

        HistoryEmptyContainer.IsVisible =
            false;

        HistoryListContainer.IsVisible =
            false;

        HistoryErrorLabel.Text =
            message;

        HistoryErrorContainer.IsVisible =
            true;
    }

    /// <summary>
    /// Retries loading reservation history.
    /// </summary>
    private async void OnRetryHistoryClicked(
        object sender,
        EventArgs e)
    {
        await LoadReservationHistoryAsync();
    }

    /// <summary>
    /// Opens the selected reservation.
    /// </summary>
    private async void OnHistoryReservationTapped(
        object sender,
        TappedEventArgs e)
    {
        if (e.Parameter is null ||
            !int.TryParse(
                e.Parameter.ToString(),
                out var reservationId) ||
            reservationId <= 0)
        {
            return;
        }

        await Shell.Current.GoToAsync(
            $"//ReservationDetailsPage?reservationId={reservationId}");
    }

    /// <summary>
    /// Returns to the vehicle inventory.
    /// </summary>
    private async void OnBackClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//VehiclesPage");
    }

    /// <summary>
    /// Retries loading the vehicle.
    /// </summary>
    private async void OnRetryClicked(
        object sender,
        EventArgs e)
    {
        await LoadVehicleAsync();
    }

    /// <summary>
    /// Displays the vehicle loading state.
    /// </summary>
    private void ShowLoading()
    {
        LoadingContainer.IsVisible =
            true;

        ErrorContainer.IsVisible =
            false;

        VehicleContent.IsVisible =
            false;
    }

    /// <summary>
    /// Displays a vehicle loading error.
    /// </summary>
    private void ShowError(
        string message)
    {
        LoadingContainer.IsVisible =
            false;

        VehicleContent.IsVisible =
            false;

        ErrorMessageLabel.Text =
            message;

        ErrorContainer.IsVisible =
            true;
    }

    /// <summary>
    /// Retrieves a color from the global
    /// DriveFleet resource dictionary.
    /// </summary>
    private static Color GetColorResource(
        string resourceKey)
    {
        if (Application.Current?
                .Resources
                .TryGetValue(
                    resourceKey,
                    out var resource) == true &&
            resource is Color color)
        {
            return color;
        }

        return Colors.Transparent;
    }
}