using DriveFleet.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace DriveFleet.Mobile;

/// <summary>
/// Defines the main navigation shell of the
/// DriveFleet employee application.
/// </summary>
public partial class AppShell : Shell
{
    private bool _initialRouteResolved;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AppShell"/> class.
    /// </summary>
    public AppShell()
    {
        InitializeComponent();

        Loaded +=
            OnShellLoaded;
    }

    /// <summary>
    /// Resolves the initial application route
    /// after the Shell is fully loaded.
    /// </summary>
    private async void OnShellLoaded(
        object? sender,
        EventArgs e)
    {
        if (_initialRouteResolved)
        {
            return;
        }

        _initialRouteResolved =
            true;

        var services =
            IPlatformApplication
                .Current?
                .Services;

        var sessionService =
            services?
                .GetService<SessionService>();

        if (sessionService is null)
        {
            await GoToAsync(
                "//LoginPage");

            return;
        }

        var session =
            await sessionService.GetAsync();

        await GoToAsync(
            session is null
                ? "//LoginPage"
                : "//EmployeeHomePage");
    }
}