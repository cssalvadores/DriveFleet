using System.Globalization;
using DriveFleet.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

const string googleAuthenticationScheme = "Google";
const string googleExternalCookieScheme = "GoogleExternal";

// Add services to the container.
builder.Services.AddControllersWithViews();

var supportedCultures = new[]
{
    new CultureInfo("pt-PT")
};

builder.Services.Configure<RequestLocalizationOptions>(
    options =>
    {
        options.DefaultRequestCulture =
            new RequestCulture("pt-PT");

        options.SupportedCultures =
            supportedCultures;

        options.SupportedUICultures =
            supportedCultures;
    });

// ---------------------------------------------------------
// Authentication
// ---------------------------------------------------------

var googleClientId =
    builder.Configuration["Authentication:Google:ClientId"];

var googleClientSecret =
    builder.Configuration["Authentication:Google:ClientSecret"];

if (string.IsNullOrWhiteSpace(googleClientId))
{
    throw new InvalidOperationException(
        "The Google OAuth Client ID is not configured.");
}

if (string.IsNullOrWhiteSpace(googleClientSecret))
{
    throw new InvalidOperationException(
        "The Google OAuth Client Secret is not configured.");
}

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)

    // Main DriveFleet authentication cookie.
    .AddCookie(
        CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.LoginPath =
                "/account/login";

            options.AccessDeniedPath =
                "/account/access-denied";

            options.Cookie.Name =
                "DriveFleet.Auth";

            options.Cookie.HttpOnly =
                true;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;

            options.Cookie.SameSite =
                SameSiteMode.Lax;

            options.SlidingExpiration =
                false;
        })

    // Temporary cookie used while processing
    // the Google authentication result.
    .AddCookie(
        googleExternalCookieScheme,
        options =>
        {
            options.Cookie.Name =
                "DriveFleet.GoogleExternal";

            options.Cookie.HttpOnly =
                true;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;

            options.Cookie.SameSite =
                SameSiteMode.Lax;

            options.ExpireTimeSpan =
                TimeSpan.FromMinutes(5);

            options.SlidingExpiration =
                false;
        })

    // Google OAuth 2.0.
    .AddGoogle(
        googleAuthenticationScheme,
        options =>
        {
            options.ClientId =
                googleClientId;

            options.ClientSecret =
                googleClientSecret;

            options.SignInScheme =
                googleExternalCookieScheme;

            options.CallbackPath =
                "/signin-google";

            options.SaveTokens = 
                true;
        });

// ---------------------------------------------------------
// API clients
// ---------------------------------------------------------

builder.Services.AddHttpClient<AuthApiClient>(
    (serviceProvider, httpClient) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var apiBaseUrl =
            configuration["ApiSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new InvalidOperationException(
                "The DriveFleet API base URL is not configured.");
        }

        httpClient.BaseAddress =
            new Uri(apiBaseUrl);
    });

builder.Services.AddHttpClient<ProfileApiClient>(
    (serviceProvider, httpClient) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var apiBaseUrl =
            configuration["ApiSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new InvalidOperationException(
                "The DriveFleet API base URL is not configured.");
        }

        httpClient.BaseAddress =
            new Uri(apiBaseUrl);
    });

builder.Services.AddHttpClient<VehicleApiClient>(
    (serviceProvider, httpClient) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var apiBaseUrl =
            configuration["ApiSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new InvalidOperationException(
                "The DriveFleet API base URL is not configured.");
        }

        httpClient.BaseAddress =
            new Uri(apiBaseUrl);
    });

builder.Services.AddHttpClient<ReservationApiClient>(
    (serviceProvider, httpClient) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var apiBaseUrl =
            configuration["ApiSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new InvalidOperationException(
                "The DriveFleet API base URL is not configured.");
        }

        httpClient.BaseAddress =
            new Uri(apiBaseUrl);
    });

builder.Services.AddHttpClient<ExtraApiClient>(
    (serviceProvider, httpClient) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var apiBaseUrl =
            configuration["ApiSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new InvalidOperationException(
                "The DriveFleet API base URL is not configured.");
        }

        httpClient.BaseAddress =
            new Uri(apiBaseUrl);
    });

builder.Services.AddHttpClient<AdminReviewApiClient>(
    (serviceProvider, httpClient) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var apiBaseUrl =
            configuration["ApiSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new InvalidOperationException(
                "The DriveFleet API base URL is not configured.");
        }

        httpClient.BaseAddress =
            new Uri(apiBaseUrl);
    });

var app = builder.Build();

app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern:
            "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();