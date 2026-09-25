using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DriveFleet.Web.Services;

/// <summary>
/// Provides HTTP operations for vehicle endpoints
/// exposed by the DriveFleet API.
/// </summary>
public class VehicleApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VehicleApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// The HTTP client configured to communicate with the DriveFleet API.
    /// </param>
    public VehicleApiClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Builds the absolute public URL for a resource
    /// exposed by the DriveFleet API.
    /// </summary>
    /// <param name="relativePath">
    /// The relative resource path returned by the API.
    /// </param>
    /// <returns>
    /// The absolute public URL of the resource,
    /// or null when the path is empty or invalid.
    /// </returns>
    public string? GetPublicResourceUrl(
        string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) ||
            _httpClient.BaseAddress is null)
        {
            return null;
        }

        return new Uri(
            _httpClient.BaseAddress,
            relativePath.TrimStart('/'))
            .ToString();
    }

    /// <summary>
    /// Retrieves all vehicles from the DriveFleet API.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The vehicle list request result returned by the API.
    /// </returns>
    public async Task<VehicleListApiResult> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                "api/vehicles",
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var vehicles =
                await response.Content.ReadFromJsonAsync<List<VehicleResponse>>(
                    cancellationToken: cancellationToken);

            return new VehicleListApiResult
            {
                StatusCode = response.StatusCode,
                Vehicles = vehicles?
                    .Select(MapVehicle)
                    .ToList()
                    ?? new List<VehicleApiModel>()
            };
        }

        return new VehicleListApiResult
        {
            StatusCode = response.StatusCode,
            Detail = await ReadProblemDetailAsync(
                response,
                cancellationToken)
        };
    }

    /// <summary>
    /// Retrieves a vehicle by its identifier from the DriveFleet API.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The vehicle request result returned by the API.
    /// </returns>
    public async Task<VehicleApiResult> GetByIdAsync(
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                $"api/vehicles/{vehicleId}",
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var vehicle =
                await response.Content.ReadFromJsonAsync<VehicleResponse>(
                    cancellationToken: cancellationToken);

            return new VehicleApiResult
            {
                StatusCode = response.StatusCode,
                Vehicle = vehicle is null
                    ? null
                    : MapVehicle(vehicle)
            };
        }

        return new VehicleApiResult
        {
            StatusCode = response.StatusCode,
            Detail = await ReadProblemDetailAsync(
                response,
                cancellationToken)
        };
    }

    /// <summary>
    /// Retrieves the available vehicle categories
    /// from the DriveFleet API.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The vehicle category options returned by the API.
    /// </returns>
    public async Task<VehicleOptionsApiResult> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                "api/vehicles/categories",
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var options =
                await response.Content.ReadFromJsonAsync<List<VehicleOptionResponse>>(
                    cancellationToken: cancellationToken);

            return new VehicleOptionsApiResult
            {
                StatusCode = response.StatusCode,
                Options = options?
                    .Select(option => new VehicleOptionApiModel
                    {
                        Id = option.Id,
                        Name = option.Name
                    })
                    .ToList()
                    ?? new List<VehicleOptionApiModel>()
            };
        }

        return new VehicleOptionsApiResult
        {
            StatusCode = response.StatusCode,
            Detail = await ReadProblemDetailAsync(
                response,
                cancellationToken)
        };
    }

    /// <summary>
    /// Retrieves the available vehicle statuses
    /// from the DriveFleet API.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The vehicle status options returned by the API.
    /// </returns>
    public async Task<VehicleOptionsApiResult> GetVehicleStatusesAsync(
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                "api/vehicles/statuses",
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var options =
                await response.Content.ReadFromJsonAsync<List<VehicleOptionResponse>>(
                    cancellationToken: cancellationToken);

            return new VehicleOptionsApiResult
            {
                StatusCode = response.StatusCode,
                Options = options?
                    .Select(option => new VehicleOptionApiModel
                    {
                        Id = option.Id,
                        Name = option.Name
                    })
                    .ToList()
                    ?? new List<VehicleOptionApiModel>()
            };
        }

        return new VehicleOptionsApiResult
        {
            StatusCode = response.StatusCode,
            Detail = await ReadProblemDetailAsync(
                response,
                cancellationToken)
        };
    }

    /// <summary>
    /// Sends an authenticated vehicle creation request
    /// to the DriveFleet API.
    /// </summary>
    /// <param name="accessToken">
    /// The JWT access token of the authenticated user.
    /// </param>
    /// <param name="brand">
    /// The vehicle brand.
    /// </param>
    /// <param name="model">
    /// The vehicle model.
    /// </param>
    /// <param name="year">
    /// The vehicle year.
    /// </param>
    /// <param name="licensePlate">
    /// The vehicle license plate.
    /// </param>
    /// <param name="seats">
    /// The number of seats.
    /// </param>
    /// <param name="dailyPrice">
    /// The daily rental price.
    /// </param>
    /// <param name="description">
    /// The optional vehicle description.
    /// </param>
    /// <param name="categoryId">
    /// The vehicle category identifier.
    /// </param>
    /// <param name="vehicleStatusId">
    /// The vehicle status identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The result returned by the DriveFleet API.
    /// </returns>
    public async Task<CreateVehicleApiResult> CreateAsync(
        string accessToken,
        string brand,
        string model,
        int year,
        string licensePlate,
        int seats,
        decimal dailyPrice,
        string? description,
        int categoryId,
        int vehicleStatusId,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateVehicleRequest
        {
            Brand = brand,
            Model = model,
            Year = year,
            LicensePlate = licensePlate,
            Seats = seats,
            DailyPrice = dailyPrice,
            Description = description,
            CategoryId = categoryId,
            VehicleStatusId = vehicleStatusId
        };

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/vehicles");

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        httpRequest.Content =
            JsonContent.Create(request);

        using var response =
            await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var vehicle =
                await response.Content.ReadFromJsonAsync<VehicleResponse>(
                    cancellationToken: cancellationToken);

            return new CreateVehicleApiResult
            {
                StatusCode = response.StatusCode,
                VehicleId = vehicle?.VehicleId
            };
        }

        return new CreateVehicleApiResult
        {
            StatusCode = response.StatusCode,
            Detail = await ReadProblemDetailAsync(
                response,
                cancellationToken)
        };
    }

    /// <summary>
    /// Sends an authenticated vehicle update request
    /// to the DriveFleet API.
    /// </summary>
    /// <param name="accessToken">
    /// The JWT access token of the authenticated user.
    /// </param>
    /// <param name="vehicleId">
    /// The identifier of the vehicle to update.
    /// </param>
    /// <param name="brand">
    /// The vehicle brand.
    /// </param>
    /// <param name="model">
    /// The vehicle model.
    /// </param>
    /// <param name="year">
    /// The vehicle year.
    /// </param>
    /// <param name="licensePlate">
    /// The vehicle license plate.
    /// </param>
    /// <param name="seats">
    /// The number of seats.
    /// </param>
    /// <param name="dailyPrice">
    /// The daily rental price.
    /// </param>
    /// <param name="description">
    /// The optional vehicle description.
    /// </param>
    /// <param name="categoryId">
    /// The vehicle category identifier.
    /// </param>
    /// <param name="vehicleStatusId">
    /// The vehicle status identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The result returned by the DriveFleet API.
    /// </returns>
    public async Task<UpdateVehicleApiResult> UpdateAsync(
        string accessToken,
        int vehicleId,
        string brand,
        string model,
        int year,
        string licensePlate,
        int seats,
        decimal dailyPrice,
        string? description,
        int categoryId,
        int vehicleStatusId,
        CancellationToken cancellationToken = default)
    {
        var request = new UpdateVehicleRequest
        {
            Brand = brand,
            Model = model,
            Year = year,
            LicensePlate = licensePlate,
            Seats = seats,
            DailyPrice = dailyPrice,
            Description = description,
            CategoryId = categoryId,
            VehicleStatusId = vehicleStatusId
        };

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Put,
                $"api/vehicles/{vehicleId}");

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        httpRequest.Content =
            JsonContent.Create(request);

        using var response =
            await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

        return new UpdateVehicleApiResult
        {
            StatusCode = response.StatusCode,
            Detail = response.IsSuccessStatusCode
                ? null
                : await ReadProblemDetailAsync(
                    response,
                    cancellationToken)
        };
    }

    /// <summary>
    /// Uploads or replaces a vehicle catalog photo
    /// in the specified display position.
    /// </summary>
    /// <param name="accessToken">
    /// The JWT access token of the authenticated user.
    /// </param>
    /// <param name="vehicleId">
    /// The identifier of the vehicle that owns the photo.
    /// </param>
    /// <param name="displayOrder">
    /// The catalog position of the photo, from 1 to 3.
    /// </param>
    /// <param name="photoStream">
    /// The stream containing the vehicle photo.
    /// </param>
    /// <param name="fileName">
    /// The original name of the uploaded file.
    /// </param>
    /// <param name="contentType">
    /// The MIME content type of the uploaded file.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The result returned by the DriveFleet API.
    /// </returns>
    public async Task<VehiclePhotoApiResult> SetPhotoAsync(
        string accessToken,
        int vehicleId,
        int displayOrder,
        Stream photoStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                $"api/vehicles/{vehicleId}/photos/{displayOrder}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        using var formContent =
            new MultipartFormDataContent();

        using var fileContent =
            new StreamContent(photoStream);

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(contentType);
        }

        formContent.Add(
            fileContent,
            "photo",
            fileName);

        request.Content = formContent;

        // Sends the vehicle photo to the API as multipart/form-data.
        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var photoResponse =
                await response.Content
                    .ReadFromJsonAsync<VehiclePhotoResponse>(
                        cancellationToken: cancellationToken);

            return new VehiclePhotoApiResult
            {
                StatusCode = response.StatusCode,
                VehiclePhotoId =
                    photoResponse?.VehiclePhotoId,
                FilePath =
                    photoResponse?.FilePath,
                DisplayOrder =
                    photoResponse?.DisplayOrder
            };
        }

        return new VehiclePhotoApiResult
        {
            StatusCode = response.StatusCode,
            Detail = await ReadProblemDetailAsync(
                response,
                cancellationToken)
        };
    }

    /// <summary>
    /// Sends an authenticated request to mark a vehicle
    /// as unavailable without deleting it.
    /// </summary>
    /// <param name="accessToken">
    /// The JWT access token of the authenticated user.
    /// </param>
    /// <param name="vehicleId">
    /// The identifier of the vehicle to mark as unavailable.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The result returned by the DriveFleet API.
    /// </returns>
    public async Task<UpdateVehicleApiResult> MarkUnavailableAsync(
        string accessToken,
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"api/vehicles/{vehicleId}/unavailable");

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        return new UpdateVehicleApiResult
        {
            StatusCode = response.StatusCode,
            Detail = response.IsSuccessStatusCode
                ? null
                : await ReadProblemDetailAsync(
                    response,
                    cancellationToken)
        };
    }

    /// <summary>
    /// Reads ProblemDetails information returned by the API.
    /// </summary>
    private static async Task<string?> ReadProblemDetailAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var problemDetails =
                await response.Content.ReadFromJsonAsync<ApiProblemDetails>(
                    cancellationToken: cancellationToken);

            return problemDetails?.Detail;
        }
        catch (JsonException)
        {
            // Keeps the detail empty if the response body is not valid JSON.
            return null;
        }
    }

    /// <summary>
    /// Maps an API vehicle response to the model consumed by the Web project.
    /// </summary>
    private static VehicleApiModel MapVehicle(
        VehicleResponse vehicle)
    {
        return new VehicleApiModel
        {
            VehicleId = vehicle.VehicleId,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            LicensePlate = vehicle.LicensePlate,
            Seats = vehicle.Seats,
            DailyPrice = vehicle.DailyPrice,
            Photos = vehicle.Photos
                .OrderBy(photo => photo.DisplayOrder)
                .Select(MapVehiclePhoto)
                .ToList(),
            Description = vehicle.Description,
            CategoryId = vehicle.CategoryId,
            CategoryName = vehicle.CategoryName,
            VehicleStatusId = vehicle.VehicleStatusId,
            VehicleStatusName = vehicle.VehicleStatusName,
            CreatedAt = vehicle.CreatedAt,
            UpdatedAt = vehicle.UpdatedAt
        };
    }

    /// <summary>
    /// Maps an API vehicle photo response to the model
    /// consumed by the Web project.
    /// </summary>
    /// <param name="photo">
    /// The vehicle photo response returned by the API.
    /// </param>
    /// <returns>
    /// The vehicle photo model consumed by the Web project.
    /// </returns>
    private static VehiclePhotoApiModel MapVehiclePhoto(
        VehiclePhotoResponse photo)
    {
        return new VehiclePhotoApiModel
        {
            VehiclePhotoId = photo.VehiclePhotoId,
            FilePath = photo.FilePath,
            DisplayOrder = photo.DisplayOrder
        };
    }

    /// <summary>
    /// Represents a vehicle catalog photo returned by the API.
    /// </summary>
    private sealed class VehiclePhotoResponse
    {
        /// <summary>
        /// Gets or sets the identifier of the vehicle photo.
        /// </summary>
        public int VehiclePhotoId { get; set; }

        /// <summary>
        /// Gets or sets the relative path of the stored vehicle photo.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the catalog display position of the photo.
        /// </summary>
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Represents a vehicle response returned by the API.
    /// </summary>
    private sealed class VehicleResponse
    {
        public int VehicleId { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public string LicensePlate { get; set; } = string.Empty;

        public int Seats { get; set; }

        public decimal DailyPrice { get; set; }

        /// <summary>
        /// Gets or sets the catalog photos associated with the vehicle.
        /// </summary>
        public List<VehiclePhotoResponse> Photos { get; set; } = new();

        public string? Description { get; set; }        

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int VehicleStatusId { get; set; }

        public string VehicleStatusName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents a vehicle creation request sent to the API.
    /// </summary>
    private sealed class CreateVehicleRequest
    {
        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public string LicensePlate { get; set; } = string.Empty;

        public int Seats { get; set; }

        public decimal DailyPrice { get; set; }

        public string? Description { get; set; }

        public int CategoryId { get; set; }

        public int VehicleStatusId { get; set; }
    }

    /// <summary>
    /// Represents a vehicle update request sent to the API.
    /// </summary>
    private sealed class UpdateVehicleRequest
    {
        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public string LicensePlate { get; set; } = string.Empty;

        public int Seats { get; set; }

        public decimal DailyPrice { get; set; }

        public string? Description { get; set; }

        public int CategoryId { get; set; }

        public int VehicleStatusId { get; set; }
    }

    /// <summary>
    /// Represents a selectable vehicle option returned by the API.
    /// </summary>
    private sealed class VehicleOptionResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents ProblemDetails information returned by the API.
    /// </summary>
    private sealed class ApiProblemDetails
    {
        public string? Detail { get; set; }
    }
}

/// <summary>
/// Represents the result of a vehicle catalog photo upload.
/// </summary>
public class VehiclePhotoApiResult
{
    /// <summary>
    /// Gets or sets the HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the saved vehicle photo.
    /// </summary>
    public int? VehiclePhotoId { get; set; }

    /// <summary>
    /// Gets or sets the relative path of the saved vehicle photo.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the catalog display position of the saved photo.
    /// </summary>
    public int? DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the error detail returned by the API, when available.
    /// </summary>
    public string? Detail { get; set; }
}

/// <summary>
/// Represents a vehicle catalog photo consumed by the Web project.
/// </summary>
public class VehiclePhotoApiModel
{
    /// <summary>
    /// Gets or sets the identifier of the vehicle photo.
    /// </summary>
    public int VehiclePhotoId { get; set; }

    /// <summary>
    /// Gets or sets the relative path of the stored vehicle photo.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the catalog display position of the photo.
    /// </summary>
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Represents a vehicle consumed by the Web project.
/// </summary>
public class VehicleApiModel
{
    public int VehicleId { get; set; }

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public string LicensePlate { get; set; } = string.Empty;

    public int Seats { get; set; }

    public decimal DailyPrice { get; set; }

    /// <summary>
    /// Gets or sets the catalog photos associated with the vehicle.
    /// </summary>
    public List<VehiclePhotoApiModel> Photos { get; set; } = new();

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int VehicleStatusId { get; set; }

    public string VehicleStatusName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents the result of a vehicle list API request.
/// </summary>
public class VehicleListApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public List<VehicleApiModel> Vehicles { get; set; } = new();

    public string? Detail { get; set; }
}

/// <summary>
/// Represents the result of a single vehicle API request.
/// </summary>
public class VehicleApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public VehicleApiModel? Vehicle { get; set; }

    public string? Detail { get; set; }
}

/// <summary>
/// Represents the result of a vehicle creation API request.
/// </summary>
public class CreateVehicleApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public int? VehicleId { get; set; }

    public string? Detail { get; set; }
}

/// <summary>
/// Represents the result of a vehicle update API request.
/// </summary>
public class UpdateVehicleApiResult
{
    /// <summary>
    /// Gets or sets the HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error detail returned by the API, when available.
    /// </summary>
    public string? Detail { get; set; }
}

/// <summary>
/// Represents a selectable vehicle option consumed by the Web project.
/// </summary>
public class VehicleOptionApiModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents the result of a vehicle options API request.
/// </summary>
public class VehicleOptionsApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public List<VehicleOptionApiModel> Options { get; set; } = new();

    public string? Detail { get; set; }
}