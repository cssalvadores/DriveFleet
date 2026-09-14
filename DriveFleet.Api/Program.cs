using System.Security.Claims;
using System.Text;
using DriveFleet.Application;
using DriveFleet.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Registers MVC controllers used by the REST API.
builder.Services.AddControllers();

// Registers OpenAPI support for development documentation.
builder.Services.AddOpenApi();

// Registers the DriveFleet application layer.
builder.Services.AddApplication(
    builder.Configuration);

// Registers the DriveFleet infrastructure layer.
builder.Services.AddInfrastructure(
    builder.Configuration);

// Reads the JWT configuration required to validate access tokens.
var jwtIssuer =
    builder.Configuration["Jwt:Issuer"];

var jwtAudience =
    builder.Configuration["Jwt:Audience"];

var jwtSecretKey =
    builder.Configuration["Jwt:SecretKey"];

// Fails immediately during application startup when
// required JWT configuration is missing.
if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "JWT issuer is not configured.");
}

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT audience is not configured.");
}

if (string.IsNullOrWhiteSpace(jwtSecretKey))
{
    throw new InvalidOperationException(
        "JWT secret key is not configured.");
}

// Configures JWT Bearer as the default authentication mechanism.
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // Ensures that the token was issued by DriveFleet.Api.
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                // Ensures that the token is intended for DriveFleet clients.
                ValidateAudience = true,
                ValidAudience = jwtAudience,

                // Rejects expired tokens.
                ValidateLifetime = true,

                // Verifies that the JWT signature was created
                // using the configured DriveFleet secret key.
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSecretKey)),

                // Defines which claims ASP.NET Core uses
                // for the user identifier and role.
                NameClaimType =
                    ClaimTypes.NameIdentifier,

                RoleClaimType =
                    ClaimTypes.Role,

                // Makes token expiration exact instead of allowing
                // the default additional clock tolerance.
                ClockSkew = TimeSpan.Zero
            };
    });

// Registers ASP.NET Core authorization services.
builder.Services.AddAuthorization();

var app = builder.Build();

// Exposes the generated OpenAPI document during development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Redirects HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Authentication must run before authorization.
app.UseAuthentication();
app.UseAuthorization();

// Maps controller routes such as /api/auth/login.
app.MapControllers();

app.Run();
