using System.Net;
using DriveFleet.Mobile.Models.Vehicles;
using DriveFleet.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace DriveFleet.Mobile.Pages;

/// <summary>
/// Displays the DriveFleet vehicle inventory
/// for authenticated employees.
/// </summary>
public partial class VehiclesPage : ContentPage
{
    private readonly List<VehicleListItemModel>
        _allVehicles = new();

    private bool _isLoading;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VehiclesPage"/> class.
    /// </summary>
    public VehiclesPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Refreshes the fleet whenever
    /// the page becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadVehiclesAsync();
    }

    /// <summary>
    /// Retrieves the current fleet from
    /// the DriveFleet API.
    /// </summary>
    private async Task LoadVehiclesAsync()
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
                await vehicleApiService.GetAllAsync();

            if (result.StatusCode !=
                HttpStatusCode.OK)
            {
                ShowError(
                    string.IsNullOrWhiteSpace(
                        result.Detail)
                        ? "The fleet could not be loaded."
                        : result.Detail);

                return;
            }

            _allVehicles.Clear();

            _allVehicles.AddRange(
                result.Vehicles
                    .OrderBy(vehicle =>
                        vehicle.Brand)
                    .ThenBy(vehicle =>
                        vehicle.Model)
                    .Select(vehicle =>
                        MapVehicle(
                            vehicle,
                            vehicleApiService)));

            ApplyFilter(
                SearchEntry.Text);
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
    /// Maps an API vehicle to the model displayed
    /// by the fleet page.
    /// </summary>
    private static VehicleListItemModel MapVehicle(
        VehicleModel vehicle,
        VehicleApiService vehicleApiService)
    {
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

        return new VehicleListItemModel
        {
            VehicleId =
                vehicle.VehicleId,

            Brand =
                vehicle.Brand,

            Model =
                vehicle.Model,

            Year =
                vehicle.Year,

            LicensePlate =
                vehicle.LicensePlate,

            Seats =
                vehicle.Seats,

            DailyPrice =
                vehicle.DailyPrice,

            CategoryName =
                vehicle.CategoryName,

            VehicleStatusId =
                vehicle.VehicleStatusId,

            VehicleStatusName =
                vehicle.VehicleStatusName,

            MainPhotoUrl =
                vehicleApiService
                    .GetPublicResourceUrl(
                        mainPhoto?.FilePath)
        };
    }

    /// <summary>
    /// Filters the fleet using brand, model,
    /// license plate, category or status.
    /// </summary>
    private void ApplyFilter(
        string? searchText)
    {
        var search =
            searchText?
                .Trim();

        var filteredVehicles =
            string.IsNullOrWhiteSpace(
        search)
        ? _allVehicles.ToList()
        : _allVehicles
            .Where(vehicle =>
                ContainsSearch(
                    vehicle.Brand,
                    search) ||
                ContainsSearch(
                    vehicle.Model,
                    search) ||
                ContainsSearch(
                    vehicle.LicensePlate,
                    search) ||
                ContainsSearch(
                    vehicle.CategoryName,
                    search) ||
                ContainsSearch(
                    vehicle.VehicleStatusName,
                    search))
            .ToList();

        VehiclesCollectionView.ItemsSource =
            filteredVehicles;

        VehicleCountLabel.Text =
            filteredVehicles.Count == 1
                ? "1 vehicle"
                : $"{filteredVehicles.Count} vehicles";

        var hasVehicles =
            filteredVehicles.Count > 0;

        VehiclesCollectionView.IsVisible =
            hasVehicles;

        EmptyContainer.IsVisible =
            !hasVehicles;

        ErrorContainer.IsVisible =
            false;
    }

    /// <summary>
    /// Performs a case-insensitive search
    /// against a vehicle field.
    /// </summary>
    private static bool ContainsSearch(
        string? value,
        string search)
    {
        return !string.IsNullOrWhiteSpace(
                   value) &&
               value.Contains(
                   search,
                   StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Handles changes to the fleet search field.
    /// </summary>
    private void OnSearchTextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        ApplyFilter(
            e.NewTextValue);
    }

    /// <summary>
    /// Retries the fleet request after
    /// an API or network error.
    /// </summary>
    private async void OnRetryClicked(
        object sender,
        EventArgs e)
    {
        await LoadVehiclesAsync();
    }

    /// <summary>
    /// Opens the details page for the
    /// selected fleet vehicle.
    /// </summary>
    private async void OnVehicleTapped(
        object sender,
        TappedEventArgs e)
    {
        if (e.Parameter is null ||
            !int.TryParse(
                e.Parameter.ToString(),
                out var vehicleId) ||
            vehicleId <= 0)
        {
            return;
        }

        await Shell.Current.GoToAsync(
            $"//VehicleDetailsPage?vehicleId={vehicleId}");
    }

    /// <summary>
    /// Returns to the employee home page.
    /// </summary>
    private async void OnBackClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//EmployeeHomePage");
    }

    /// <summary>
    /// Displays the loading state.
    /// </summary>
    private void ShowLoading()
    {
        LoadingContainer.IsVisible =
            true;

        ErrorContainer.IsVisible =
            false;

        EmptyContainer.IsVisible =
            false;

        VehiclesCollectionView.IsVisible =
            false;

        VehicleCountLabel.Text =
            "Loading vehicles...";
    }

    /// <summary>
    /// Displays a fleet loading error.
    /// </summary>
    private void ShowError(
        string message)
    {
        LoadingContainer.IsVisible =
            false;

        VehiclesCollectionView.IsVisible =
            false;

        EmptyContainer.IsVisible =
            false;

        ErrorMessageLabel.Text =
            message;

        ErrorContainer.IsVisible =
            true;

        VehicleCountLabel.Text =
            "Fleet unavailable";
    }
}