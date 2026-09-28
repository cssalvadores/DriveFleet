using Microsoft.Maui;
using System.Globalization;
using DriveFleet.Mobile.Models.Auth;
using DriveFleet.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DriveFleet.Mobile.Pages;

/// <summary>
/// Provides the main operational workspace
/// for an authenticated DriveFleet employee.
/// </summary>
public partial class EmployeeHomePage : ContentPage
{
    private bool _isLoadingSession;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EmployeeHomePage"/> class.
    /// </summary>
    public EmployeeHomePage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Restores the authenticated employee session
    /// whenever the home page becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoadingSession)
        {
            return;
        }

        try
        {
            _isLoadingSession =
                true;

            var sessionService =
                IPlatformApplication
                    .Current?
                    .Services
                    .GetService<SessionService>();

            if (sessionService is null)
            {
                await ReturnToLoginAsync();

                return;
            }

            var session =
                await sessionService.GetAsync();

            if (session is null)
            {
                await ReturnToLoginAsync();

                return;
            }

            PopulateEmployeeInformation(
                session);
        }
        finally
        {
            _isLoadingSession =
                false;
        }
    }

    /// <summary>
    /// Populates the employee information displayed
    /// in the premium home interface.
    /// </summary>
    private void PopulateEmployeeInformation(
        EmployeeSession session)
    {
        GreetingLabel.Text =
            GetGreeting();

        EmployeeNameLabel.Text =
            string.IsNullOrWhiteSpace(
                session.FirstName)
                ? "Employee"
                : session.FirstName;

        EmployeeEmailLabel.Text =
            session.Email;

        InitialsLabel.Text =
            BuildInitials(
                session.FirstName,
                session.LastName);

        CurrentDateLabel.Text =
            DateTime.Now.ToString(
                "dddd, dd MMMM yyyy",
                CultureInfo.GetCultureInfo(
                    "en-GB"));
    }

    /// <summary>
    /// Returns an appropriate greeting based
    /// on the user's local time.
    /// </summary>
    private static string GetGreeting()
    {
        var hour =
            DateTime.Now.Hour;

        return hour switch
        {
            < 12 => "Good morning",
            < 18 => "Good afternoon",
            _ => "Good evening"
        };
    }

    /// <summary>
    /// Builds employee initials for the
    /// profile indicator.
    /// </summary>
    private static string BuildInitials(
        string firstName,
        string? lastName)
    {
        var firstInitial =
            string.IsNullOrWhiteSpace(
                firstName)
                ? string.Empty
                : firstName[..1];

        var lastInitial =
            string.IsNullOrWhiteSpace(
                lastName)
                ? string.Empty
                : lastName[..1];

        var initials =
            $"{firstInitial}{lastInitial}"
                .ToUpperInvariant();

        return string.IsNullOrWhiteSpace(
            initials)
                ? "DF"
                : initials;
    }

    /// <summary>
    /// Revokes the current access token,
    /// clears the local employee session,
    /// and returns to the login page.
    /// </summary>
    private async void OnSignOutClicked(
        object sender,
        EventArgs e)
    {
        SignOutButton.IsEnabled =
            false;

        var services =
            IPlatformApplication
                .Current?
                .Services;

        var sessionService =
            services?
                .GetService<SessionService>();

        var authApiService =
            services?
                .GetService<AuthApiService>();

        if (sessionService is null)
        {
            SignOutButton.IsEnabled =
                true;

            return;
        }

        try
        {
            var session =
                await sessionService.GetAsync();

            if (session is not null &&
                authApiService is not null &&
                !string.IsNullOrWhiteSpace(
                    session.AccessToken))
            {
                try
                {
                    await authApiService.LogoutAsync(
                        session.AccessToken);
                }
                catch (HttpRequestException)
                {
                    // Local logout must still succeed
                    // when the API cannot be reached.
                }
            }

            sessionService.Clear();

            await Shell.Current.GoToAsync(
                "//LoginPage");
        }
        finally
        {
            SignOutButton.IsEnabled =
                true;
        }
    }

    /// <summary>
    /// Returns the user to the login page
    /// when no valid employee session exists.
    /// </summary>
    private static Task ReturnToLoginAsync()
    {
        return Shell.Current.GoToAsync(
            "//LoginPage");
    }
}