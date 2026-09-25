using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using DriveFleet.Web.Services;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.AccessDeniedPath = "/account/access-denied";

        options.Cookie.Name = "DriveFleet.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;
        options.Cookie.SameSite =
            SameSiteMode.Lax;

        options.SlidingExpiration = false;
    });

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

var app = builder.Build();

app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
