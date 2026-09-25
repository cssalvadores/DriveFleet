using DriveFleet.Web.Models.Vehicles;
using DriveFleet.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;

namespace DriveFleet.Web.Controllers;

/// <summary>
/// Provides public vehicle-related pages.
/// </summary>
public class VehiclesController : Controller
{
    private readonly VehicleApiClient _vehicleApiClient;
    private readonly ILogger<VehiclesController> _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VehiclesController"/> class.
    /// </summary>
    /// <param name="vehicleApiClient">
    /// Client used to communicate with vehicle endpoints
    /// exposed by the DriveFleet API.
    /// </param>
    /// <param name="logger">
    /// Logger used to record unexpected communication errors.
    /// </param>
    public VehiclesController(
        VehicleApiClient vehicleApiClient,
        ILogger<VehiclesController> logger)
    {
        _vehicleApiClient = vehicleApiClient;
        _logger = logger;
    }

    /// <summary>
    /// Displays the public list of vehicles.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The vehicle list page.
    /// </returns>
    [HttpGet("/vehicles")]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _vehicleApiClient.GetAllAsync(
                    cancellationToken);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return View("VehicleError");
            }

            var model =
                result.Vehicles
                    .Select(vehicle => new VehicleListItemViewModel
                    {
                        VehicleId = vehicle.VehicleId,
                        Brand = vehicle.Brand,
                        Model = vehicle.Model,
                        Year = vehicle.Year,
                        Seats = vehicle.Seats,
                        DailyPrice = vehicle.DailyPrice,
                        MainPhotoUrl =
                            _vehicleApiClient.GetPublicResourceUrl(
                                vehicle.Photos
                                    .FirstOrDefault(
                                         photo => photo.DisplayOrder == 1)
                                    ?.FilePath),
                        Description = vehicle.Description,
                        CategoryName = vehicle.CategoryName,
                        VehicleStatusName = vehicle.VehicleStatusName
                    })
                    .ToList();

            return View(model);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while retrieving vehicles.");

            return View("VehicleError");
        }
    }

    /// <summary>
    /// Displays the details of a vehicle.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle to display.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The vehicle details page when the vehicle exists.
    /// </returns>
    [HttpGet("/vehicles/{vehicleId:int}")]
    public async Task<IActionResult> Details(
        int vehicleId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _vehicleApiClient.GetByIdAsync(
                    vehicleId,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.NotFound ||
                result.Vehicle is null)
            {
                return NotFound();
            }

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return View("VehicleError");
            }

            var model = new VehicleDetailsViewModel
            {
                VehicleId = result.Vehicle.VehicleId,
                Brand = result.Vehicle.Brand,
                Model = result.Vehicle.Model,
                Year = result.Vehicle.Year,
                LicensePlate = result.Vehicle.LicensePlate,
                Seats = result.Vehicle.Seats,
                DailyPrice = result.Vehicle.DailyPrice,
                Photos = MapVehiclePhotos(
                    result.Vehicle.Photos),
                Description = result.Vehicle.Description,
                CategoryName = result.Vehicle.CategoryName,
                VehicleStatusName = result.Vehicle.VehicleStatusName
            };

            return View(model);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while retrieving vehicle {VehicleId}.",
                vehicleId);

            return View("VehicleError");
        }
    }

    /// <summary>
    /// Displays the vehicle creation form for authorized staff.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The vehicle creation page when the required options are loaded successfully.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("/vehicles/create")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        try
        {
            var categoriesResult =
                await _vehicleApiClient.GetCategoriesAsync(
                    cancellationToken);

            var statusesResult =
                await _vehicleApiClient.GetVehicleStatusesAsync(
                    cancellationToken);

            if (categoriesResult.StatusCode != HttpStatusCode.OK ||
                statusesResult.StatusCode != HttpStatusCode.OK)
            {
                return View("VehicleError");
            }

            var model = new CreateVehicleViewModel
            {
                Categories = categoriesResult.Options
                    .Select(option => new SelectListItem
                    {
                        Value = option.Id.ToString(),
                        Text = option.Name
                    })
                    .ToList(),

                VehicleStatuses = statusesResult.Options
                    .Select(option => new SelectListItem
                    {
                        Value = option.Id.ToString(),
                        Text = option.Name
                    })
                    .ToList()
            };

            return View(model);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while preparing the vehicle creation form.");

            return View("VehicleError");
        }
    }

    /// <summary>
    /// Processes the vehicle creation form for authorized staff.
    /// </summary>
    /// <param name="viewModel">
    /// The vehicle information entered by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// A redirect to the vehicle details page when creation succeeds,
    /// or the creation form when validation fails.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpPost("/vehicles/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateVehicleViewModel viewModel,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var optionsLoaded =
                    await PopulateCreateOptionsAsync(
                        viewModel,
                        cancellationToken);

                if (!optionsLoaded)
                {
                    return View("VehicleError");
                }

                return View(viewModel);
            }

            var accessToken =
                await HttpContext.GetTokenAsync(
                    "access_token");

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var result =
                await _vehicleApiClient.CreateAsync(
                    accessToken,
                    viewModel.Brand,
                    viewModel.Model,
                    viewModel.Year,
                    viewModel.LicensePlate,
                    viewModel.Seats,
                    viewModel.DailyPrice,
                    viewModel.Description,
                    viewModel.CategoryId,
                    viewModel.VehicleStatusId,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.Created &&
                result.VehicleId.HasValue)
            {
                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        vehicleId = result.VehicleId.Value
                    });
            }

            if (result.StatusCode == HttpStatusCode.BadRequest ||
                result.StatusCode == HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(result.Detail)
                        ? "The vehicle information is invalid."
                        : result.Detail);

                var optionsLoaded =
                    await PopulateCreateOptionsAsync(
                        viewModel,
                        cancellationToken);

                if (!optionsLoaded)
                {
                    return View("VehicleError");
                }

                return View(viewModel);
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            return View("VehicleError");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while creating a vehicle.");

            return View("VehicleError");
        }
    }

    /// <summary>
    /// Displays the vehicle editing form for authorized staff.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle to edit.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The vehicle editing page when the vehicle exists.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("/vehicles/{vehicleId:int}/edit")]
    public async Task<IActionResult> Edit(
        int vehicleId,
        CancellationToken cancellationToken)
    {
        try
        {
            var vehicleResult =
                await _vehicleApiClient.GetByIdAsync(
                    vehicleId,
                    cancellationToken);

            if (vehicleResult.StatusCode == HttpStatusCode.NotFound ||
                vehicleResult.Vehicle is null)
            {
                return NotFound();
            }

            if (vehicleResult.StatusCode != HttpStatusCode.OK)
            {
                return View("VehicleError");
            }

            var categoriesResult =
                await _vehicleApiClient.GetCategoriesAsync(
                    cancellationToken);

            var statusesResult =
                await _vehicleApiClient.GetVehicleStatusesAsync(
                    cancellationToken);

            if (categoriesResult.StatusCode != HttpStatusCode.OK ||
                statusesResult.StatusCode != HttpStatusCode.OK)
            {
                return View("VehicleError");
            }

            var vehicle = vehicleResult.Vehicle;

            var viewModel = new EditVehicleViewModel
            {
                VehicleId = vehicle.VehicleId,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                LicensePlate = vehicle.LicensePlate,
                Seats = vehicle.Seats,
                DailyPrice = vehicle.DailyPrice,
                Description = vehicle.Description,
                CategoryId = vehicle.CategoryId,
                VehicleStatusId = vehicle.VehicleStatusId,
                Photos = MapVehiclePhotos(
                        vehicle.Photos),

                Categories = categoriesResult.Options
                    .Select(option => new SelectListItem
                    {
                        Value = option.Id.ToString(),
                        Text = option.Name
                    })
                    .ToList(),

                VehicleStatuses = statusesResult.Options
                    .Select(option => new SelectListItem
                    {
                        Value = option.Id.ToString(),
                        Text = option.Name
                    })
                    .ToList()
            };

            return View(viewModel);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while preparing vehicle {VehicleId} for editing.",
                vehicleId);

            return View("VehicleError");
        }
    }

    /// <summary>
    /// Processes the vehicle editing form for authorized staff.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle to update.
    /// </param>
    /// <param name="viewModel">
    /// The updated vehicle information entered by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// A redirect to the vehicle details page when the update succeeds,
    /// or the editing form when validation fails.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpPost("/vehicles/{vehicleId:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int vehicleId,
        EditVehicleViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (vehicleId != viewModel.VehicleId)
        {
            return BadRequest();
        }

        try
        {
            if (!ModelState.IsValid)
            {
                var optionsLoaded =
                    await PopulateEditOptionsAsync(
                        viewModel,
                        cancellationToken);

                if (!optionsLoaded)
                {
                    return View("VehicleError");
                }

                return View(viewModel);
            }

            var accessToken =
                await HttpContext.GetTokenAsync(
                    "access_token");

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var result =
                await _vehicleApiClient.UpdateAsync(
                    accessToken,
                    vehicleId,
                    viewModel.Brand,
                    viewModel.Model,
                    viewModel.Year,
                    viewModel.LicensePlate,
                    viewModel.Seats,
                    viewModel.DailyPrice,
                    viewModel.Description,
                    viewModel.CategoryId,
                    viewModel.VehicleStatusId,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.NoContent)
            {
                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        vehicleId
                    });
            }

            if (result.StatusCode == HttpStatusCode.BadRequest ||
                result.StatusCode == HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(result.Detail)
                        ? "The vehicle information is invalid."
                        : result.Detail);

                var optionsLoaded =
                    await PopulateEditOptionsAsync(
                        viewModel,
                        cancellationToken);

                if (!optionsLoaded)
                {
                    return View("VehicleError");
                }

                return View(viewModel);
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            return View("VehicleError");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while updating vehicle {VehicleId}.",
                vehicleId);

            return View("VehicleError");
        }
    }

    /// <summary>
    /// Uploads or replaces a vehicle catalog photo
    /// in the specified display position.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle that owns the photo.
    /// </param>
    /// <param name="displayOrder">
    /// The catalog position of the photo, from 1 to 3.
    /// </param>
    /// <param name="photo">
    /// The vehicle photo selected by the authorized user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// A redirect to the vehicle editing page when the upload succeeds.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpPost("/vehicles/{vehicleId:int}/photos/{displayOrder:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPhoto(
        int vehicleId,
        int displayOrder,
        IFormFile photo,
        CancellationToken cancellationToken)
    {
        if (displayOrder is < 1 or > 3)
        {
            return BadRequest();
        }

        try
        {
            var accessToken =
                await HttpContext.GetTokenAsync(
                    "access_token");

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (photo is null || photo.Length == 0)
            {
                TempData["VehiclePhotoError"] =
                    "Please select a vehicle photo.";

                return RedirectToAction(
                    nameof(Edit),
                    new
                    {
                        vehicleId
                    });
            }

            await using var photoStream =
                photo.OpenReadStream();

            var result =
                await _vehicleApiClient.SetPhotoAsync(
                    accessToken,
                    vehicleId,
                    displayOrder,
                    photoStream,
                    photo.FileName,
                    photo.ContentType,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                TempData["VehiclePhotoSuccess"] =
                    $"Photo {displayOrder} was updated successfully.";

                return RedirectToAction(
                    nameof(Edit),
                    new
                    {
                        vehicleId
                    });
            }

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                TempData["VehiclePhotoError"] =
                    string.IsNullOrWhiteSpace(result.Detail)
                        ? "The selected vehicle photo is invalid."
                        : result.Detail;

                return RedirectToAction(
                    nameof(Edit),
                    new
                    {
                        vehicleId
                    });
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            return View("VehicleError");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while uploading photo {DisplayOrder} for vehicle {VehicleId}.",
                displayOrder,
                vehicleId);

            return View("VehicleError");
        }
    }

    /// <summary>
    /// Marks an existing vehicle as unavailable for authorized staff.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle to mark as unavailable.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// A redirect to the vehicle details page when the operation succeeds.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpPost("/vehicles/{vehicleId:int}/unavailable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkUnavailable(
        int vehicleId,
        CancellationToken cancellationToken)
    {
        try
        {
            var accessToken =
                await HttpContext.GetTokenAsync(
                    "access_token");

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var result =
                await _vehicleApiClient.MarkUnavailableAsync(
                    accessToken,
                    vehicleId,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.NoContent)
            {
                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        vehicleId
                    });
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            return View("VehicleError");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while marking vehicle {VehicleId} as unavailable.",
                vehicleId);

            return View("VehicleError");
        }
    }

    /// <summary>
    /// Maps vehicle catalog photos returned by the API
    /// to the view models consumed by the Web application.
    /// </summary>
    /// <param name="photos">
    /// The vehicle catalog photos returned by the API.
    /// </param>
    /// <returns>
    /// The mapped vehicle catalog photos ordered by display position.
    /// </returns>
    private List<VehiclePhotoViewModel> MapVehiclePhotos(
        IEnumerable<VehiclePhotoApiModel> photos)
    {
        return photos
            .OrderBy(photo => photo.DisplayOrder)
            .Select(photo => new VehiclePhotoViewModel
            {
                VehiclePhotoId = photo.VehiclePhotoId,
                FilePath = photo.FilePath,
                ImageUrl =
                    _vehicleApiClient.GetPublicResourceUrl(
                        photo.FilePath)
                    ?? string.Empty,
                DisplayOrder = photo.DisplayOrder
            })
            .ToList();
    }

    /// <summary>
    /// Loads the category and vehicle status options
    /// required by the vehicle editing form.
    /// </summary>
    /// <param name="viewModel">
    /// The vehicle editing view model to populate.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// True when both option lists are loaded successfully;
    /// otherwise, false.
    /// </returns>
    private async Task<bool> PopulateEditOptionsAsync(
        EditVehicleViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var categoriesResult =
            await _vehicleApiClient.GetCategoriesAsync(
                cancellationToken);

        var statusesResult =
            await _vehicleApiClient.GetVehicleStatusesAsync(
                cancellationToken);

        if (categoriesResult.StatusCode != HttpStatusCode.OK ||
            statusesResult.StatusCode != HttpStatusCode.OK)
        {
            return false;
        }

        viewModel.Categories =
            categoriesResult.Options
                .Select(option => new SelectListItem
                {
                    Value = option.Id.ToString(),
                    Text = option.Name
                })
                .ToList();

        viewModel.VehicleStatuses =
            statusesResult.Options
                .Select(option => new SelectListItem
                {
                    Value = option.Id.ToString(),
                    Text = option.Name
                })
                .ToList();

        return true;
    }

    /// <summary>
    /// Loads the category and vehicle status options
    /// required by the vehicle creation form.
    /// </summary>
    /// <param name="model">
    /// The vehicle creation view model to populate.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// True when both option lists are loaded successfully;
    /// otherwise, false.
    /// </returns>
    private async Task<bool> PopulateCreateOptionsAsync(
        CreateVehicleViewModel model,
        CancellationToken cancellationToken)
    {
        var categoriesResult =
            await _vehicleApiClient.GetCategoriesAsync(
                cancellationToken);

        var statusesResult =
            await _vehicleApiClient.GetVehicleStatusesAsync(
                cancellationToken);

        if (categoriesResult.StatusCode != HttpStatusCode.OK ||
            statusesResult.StatusCode != HttpStatusCode.OK)
        {
            return false;
        }

        model.Categories =
            categoriesResult.Options
                .Select(option => new SelectListItem
                {
                    Value = option.Id.ToString(),
                    Text = option.Name
                })
                .ToList();

        model.VehicleStatuses =
            statusesResult.Options
                .Select(option => new SelectListItem
                {
                    Value = option.Id.ToString(),
                    Text = option.Name
                })
                .ToList();

        return true;
    }
}
