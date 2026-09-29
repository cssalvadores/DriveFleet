using System.Net;
using DriveFleet.Mobile.Models.Reservations;
using DriveFleet.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace DriveFleet.Mobile.Pages;

/// <summary>
/// Displays detailed reservation information
/// and employee workflow actions.
/// </summary>
public partial class ReservationDetailsPage :
    ContentPage,
    IQueryAttributable
{
    private const string PendingStatus =
        "Pending";

    private const string ActiveStatus =
        "Active";

    private const string CompletedStatus =
        "Completed";

    private const string CancelledStatus =
        "Cancelled";

    private int _reservationId;

    private bool _isLoading;

    private bool _isProcessingAction;

    private ReservationModel? _currentReservation;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReservationDetailsPage"/> class.
    /// </summary>
    public ReservationDetailsPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Receives the selected reservation identifier
    /// from Shell navigation.
    /// </summary>
    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (!query.TryGetValue(
                "reservationId",
                out var reservationIdValue))
        {
            return;
        }

        if (!int.TryParse(
                reservationIdValue?.ToString(),
                out var reservationId))
        {
            return;
        }

        _reservationId =
            reservationId;
    }

    /// <summary>
    /// Refreshes the reservation when
    /// the page becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_reservationId <= 0)
        {
            ShowError(
                "A valid reservation was not selected.");

            return;
        }

        await LoadReservationAsync();
    }

    /// <summary>
    /// Retrieves the selected reservation
    /// from the DriveFleet API.
    /// </summary>
    private async Task LoadReservationAsync()
    {
        if (_isLoading)
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
            ShowError(
                "The reservation service is unavailable.");

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
            _isLoading =
                true;

            ShowLoading();

            var result =
                await reservationApiService.GetByIdAsync(
                    session.AccessToken,
                    _reservationId);

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
                ShowError(
                    "You are not authorized to view this reservation.");

                return;
            }

            if (result.StatusCode ==
                    HttpStatusCode.NotFound ||
                result.Reservation is null)
            {
                ShowError(
                    "The selected reservation could not be found.");

                return;
            }

            if (result.StatusCode !=
                HttpStatusCode.OK)
            {
                ShowError(
                    string.IsNullOrWhiteSpace(
                        result.Detail)
                        ? "The reservation could not be loaded."
                        : result.Detail);

                return;
            }

            PopulateReservation(
                result.Reservation);
        }
        catch (HttpRequestException)
        {
            ShowError(
                "Unable to connect to the DriveFleet API.");
        }
        catch (TaskCanceledException)
        {
            ShowError(
                "The reservation request timed out.");
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
    /// Populates the reservation details interface.
    /// </summary>
    private void PopulateReservation(
        ReservationModel reservation)
    {
        _currentReservation =
            reservation;

        BindingContext =
            reservation;

        ReservationNumberHeaderLabel.Text =
            $"Reservation #{reservation.ReservationId}";

        ReservationNumberLabel.Text =
            $"Reservation #{reservation.ReservationId}";

        CustomerNameLabel.Text =
            reservation.UserFullName;

        CustomerEmailLabel.Text =
            reservation.UserEmail;

        TotalValueLabel.Text =
            $"€{reservation.TotalValue:F2}";

        StatusLabel.Text =
            reservation.ReservationStatusName
                .ToUpperInvariant();

        ApplyStatusAppearance(
            reservation.ReservationStatusName);

        ConfigureWorkflow(
            reservation.ReservationStatusName);

        ErrorContainer.IsVisible =
            false;

        ReservationContent.IsVisible =
            true;
    }

    /// <summary>
    /// Configures the actions available
    /// for the reservation current status.
    /// </summary>
    private void ConfigureWorkflow(
        string statusName)
    {
        StartReservationButton.IsVisible =
            string.Equals(
                statusName,
                PendingStatus,
                StringComparison.OrdinalIgnoreCase);

        CompleteReservationButton.IsVisible =
            string.Equals(
                statusName,
                ActiveStatus,
                StringComparison.OrdinalIgnoreCase);

        var isReadOnly =
            string.Equals(
                statusName,
                CompletedStatus,
                StringComparison.OrdinalIgnoreCase)
            ||
            string.Equals(
                statusName,
                CancelledStatus,
                StringComparison.OrdinalIgnoreCase);

        ReadOnlyStatusLabel.IsVisible =
            isReadOnly;

        if (string.Equals(
            statusName,
            CompletedStatus,
            StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlyStatusLabel.Text =
                "This reservation has been completed.";
        }
        else if (string.Equals(
            statusName,
            CancelledStatus,
            StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlyStatusLabel.Text =
                "This reservation has been cancelled.";
        }
    }

    /// <summary>
    /// Applies DriveFleet visual colors
    /// to the reservation status.
    /// </summary>
    private void ApplyStatusAppearance(
        string statusName)
    {
        var backgroundKey =
            "DriveFleetAccentSoft";

        var foregroundKey =
            "DriveFleetAccent";

        if (string.Equals(
            statusName,
            PendingStatus,
            StringComparison.OrdinalIgnoreCase))
        {
            backgroundKey =
                "DriveFleetWarningSoft";

            foregroundKey =
                "DriveFleetWarning";
        }
        else if (string.Equals(
            statusName,
            ActiveStatus,
            StringComparison.OrdinalIgnoreCase))
        {
            backgroundKey =
                "DriveFleetInfoSoft";

            foregroundKey =
                "DriveFleetInfo";
        }
        else if (string.Equals(
            statusName,
            CompletedStatus,
            StringComparison.OrdinalIgnoreCase))
        {
            backgroundKey =
                "DriveFleetSuccessSoft";

            foregroundKey =
                "DriveFleetSuccess";
        }
        else if (string.Equals(
            statusName,
            CancelledStatus,
            StringComparison.OrdinalIgnoreCase))
        {
            backgroundKey =
                "DriveFleetDangerSoft";

            foregroundKey =
                "DriveFleetDanger";
        }

        StatusBorder.BackgroundColor =
            GetColorResource(
                backgroundKey);

        StatusLabel.TextColor =
            GetColorResource(
                foregroundKey);
    }

    /// <summary>
    /// Starts the selected pending reservation.
    /// </summary>
    private async void OnStartReservationClicked(
        object sender,
        EventArgs e)
    {
        await ExecuteReservationActionAsync(
            "Start reservation",
            "Start this reservation now?",
            "start");
    }

    /// <summary>
    /// Completes the selected active reservation.
    /// </summary>
    private async void OnCompleteReservationClicked(
        object sender,
        EventArgs e)
    {
        await ExecuteReservationActionAsync(
            "Complete reservation",
            "Complete this reservation now?",
            "complete");
    }

    /// <summary>
    /// Executes an authenticated reservation
    /// workflow transition.
    /// </summary>
    private async Task ExecuteReservationActionAsync(
        string title,
        string confirmationMessage,
        string actionName)
    {
        if (_isProcessingAction ||
            _currentReservation is null)
        {
            return;
        }

        var confirmed =
            await DisplayAlert(
                title,
                confirmationMessage,
                "Confirm",
                "Cancel");

        if (!confirmed)
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
            ShowActionError(
                "The reservation service is unavailable.");

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
            _isProcessingAction =
                true;

            SetActionButtonsEnabled(
                false);

            HideActionMessage();

            var result =
                string.Equals(
                    actionName,
                    "start",
                    StringComparison.OrdinalIgnoreCase)
                    ? await reservationApiService.StartAsync(
                        session.AccessToken,
                        _reservationId)
                    : await reservationApiService.CompleteAsync(
                        session.AccessToken,
                        _reservationId);

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
                ShowActionError(
                    "You are not authorized to update this reservation.");

                return;
            }

            if (result.StatusCode ==
                HttpStatusCode.NotFound)
            {
                ShowActionError(
                    "The selected reservation no longer exists.");

                return;
            }

            if (result.StatusCode ==
                HttpStatusCode.Conflict)
            {
                ShowActionError(
                    string.IsNullOrWhiteSpace(
                        result.Detail)
                        ? "This reservation transition is not allowed."
                        : result.Detail);

                return;
            }

            if (result.StatusCode !=
                HttpStatusCode.NoContent)
            {
                ShowActionError(
                    "The reservation could not be updated.");

                return;
            }

            ShowActionSuccess(
                string.Equals(
                    actionName,
                    "start",
                    StringComparison.OrdinalIgnoreCase)
                    ? "Reservation started successfully."
                    : "Reservation completed successfully.");

            await LoadReservationAsync();
        }
        catch (HttpRequestException)
        {
            ShowActionError(
                "Unable to connect to the DriveFleet API.");
        }
        catch (TaskCanceledException)
        {
            ShowActionError(
                "The reservation update request timed out.");
        }
        finally
        {
            _isProcessingAction =
                false;

            SetActionButtonsEnabled(
                true);
        }
    }

    /// <summary>
    /// Enables or disables workflow buttons.
    /// </summary>
    private void SetActionButtonsEnabled(
        bool enabled)
    {
        StartReservationButton.IsEnabled =
            enabled;

        CompleteReservationButton.IsEnabled =
            enabled;
    }

    /// <summary>
    /// Displays a successful workflow message.
    /// </summary>
    private void ShowActionSuccess(
        string message)
    {
        ActionMessageBorder.BackgroundColor =
            GetColorResource(
                "DriveFleetSuccessSoft");

        ActionMessageLabel.TextColor =
            GetColorResource(
                "DriveFleetSuccess");

        ActionMessageLabel.Text =
            message;

        ActionMessageBorder.IsVisible =
            true;
    }

    /// <summary>
    /// Displays a workflow error message.
    /// </summary>
    private void ShowActionError(
        string message)
    {
        ActionMessageBorder.BackgroundColor =
            GetColorResource(
                "DriveFleetDangerSoft");

        ActionMessageLabel.TextColor =
            GetColorResource(
                "DriveFleetDanger");

        ActionMessageLabel.Text =
            message;

        ActionMessageBorder.IsVisible =
            true;
    }

    /// <summary>
    /// Clears the current workflow message.
    /// </summary>
    private void HideActionMessage()
    {
        ActionMessageLabel.Text =
            string.Empty;

        ActionMessageBorder.IsVisible =
            false;
    }

    /// <summary>
    /// Returns to today's reservations.
    /// </summary>
    private async void OnBackClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//TodayReservationsPage");
    }

    /// <summary>
    /// Retries loading the reservation.
    /// </summary>
    private async void OnRetryClicked(
        object sender,
        EventArgs e)
    {
        await LoadReservationAsync();
    }

    /// <summary>
    /// Displays the reservation loading state.
    /// </summary>
    private void ShowLoading()
    {
        LoadingContainer.IsVisible =
            true;

        ErrorContainer.IsVisible =
            false;

        ReservationContent.IsVisible =
            false;
    }

    /// <summary>
    /// Displays a reservation loading error.
    /// </summary>
    private void ShowError(
        string message)
    {
        LoadingContainer.IsVisible =
            false;

        ReservationContent.IsVisible =
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