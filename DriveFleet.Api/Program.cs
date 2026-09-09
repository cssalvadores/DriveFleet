using DriveFleet.Application;
using DriveFleet.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


// Registers application services.
builder.Services.AddApplication(
    builder.Configuration);

// Registers persistence and external infrastructure services.
builder.Services.AddInfrastructure(
    builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
