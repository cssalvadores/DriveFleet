using System.Net;
using DriveFleet.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DriveFleet.Mobile.Pages;

/// <summary>
/// Provides the employee authentication screen.
/// </summary>
public partial class LoginPage : ContentPage
{
    private const string EmployeeRole =
        "Employee";

    private bool _isSigningIn;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="LoginPage"/> class.
    /// </summary>
    public LoginPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Validates the credentials and authenticates
    /// the employee against the DriveFleet API.
    /// </summary>
    private async void OnSignInClicked(
        object sender,
        EventArgs e)
    {
        if (_isSigningIn)
        {
            return;
        }

        HideMessage();

        var email =
            EmailEntry.Text?.Trim();

        var password =
            PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(
            email))
        {
            ShowError(
                "Enter your employee email address.");

            EmailEntry.Focus();

            return;
        }

        if (string.IsNullOrWhiteSpace(
            password))
        {
            ShowError(
                "Enter your password.");

            PasswordEntry.Focus();

            return;
        }

        var services =
            Handler?
                .MauiContext?
                .Services;

        var authApiService =
            services?
                .GetService<AuthApiService>();

        var sessionService =
            services?
                .GetService<SessionService>();

        if (authApiService is null ||
            sessionService is null)
        {
            ShowError(
                "The authentication service is unavailable.");

            return;
        }

        try
        {
            SetLoadingState(
                true);

            var result =
                await authApiService.LoginAsync(
                    email,
                    password);

            if (result.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                ShowError(
                    string.IsNullOrWhiteSpace(
                        result.Detail)
                        ? "Invalid email or password."
                        : result.Detail);

                return;
            }

            if (result.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                ShowError(
                    string.IsNullOrWhiteSpace(
                        result.Detail)
                        ? "This account cannot sign in."
                        : result.Detail);

                return;
            }

            if (result.StatusCode !=
                    HttpStatusCode.OK ||
                result.Login is null)
            {
                ShowError(
                    "We could not sign you in. Please try again.");

                return;
            }

            if (!string.Equals(
                result.Login.Role,
                EmployeeRole,
                StringComparison.OrdinalIgnoreCase))
            {
                ShowError(
                    "This application is available only to DriveFleet employees.");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    result.Login.AccessToken) ||
                result.Login.ExpiresAt <=
                    DateTime.UtcNow)
            {
                ShowError(
                    "The authentication response is invalid.");

                return;
            }

            var sessionSaved =
                await sessionService.SaveAsync(
                    result.Login);

            if (!sessionSaved)
            {
                ShowError(
                    "The employee session could not be stored securely.");

                return;
            }

            var storedSession =
                await sessionService.GetAsync();

            if (storedSession is null)
            {
                ShowError(
                    "The employee session could not be restored.");

                return;
            }

            PasswordEntry.Text =
                string.Empty;

            ShowSuccess(
                $"Welcome, {storedSession.FirstName}. " +
                "Your employee session is securely stored.");
        }
        catch (HttpRequestException)
        {
            ShowError(
                "Unable to connect to the DriveFleet API. " +
                "Confirm that the API is running.");
        }
        catch (TaskCanceledException)
        {
            ShowError(
                "The authentication request timed out. " +
                "Please try again.");
        }
        finally
        {
            SetLoadingState(
                false);
        }
    }

    /// <summary>
    /// Updates the login controls while an
    /// authentication request is running.
    /// </summary>
    private void SetLoadingState(
        bool isLoading)
    {
        _isSigningIn =
            isLoading;

        SignInButton.IsEnabled =
            !isLoading;

        EmailEntry.IsEnabled =
            !isLoading;

        PasswordEntry.IsEnabled =
            !isLoading;

        LoadingContainer.IsVisible =
            isLoading;
    }

    /// <summary>
    /// Displays an authentication error message.
    /// </summary>
    private void ShowError(
        string message)
    {
        ErrorBorder.BackgroundColor =
            GetColorResource(
                "DriveFleetDangerSoft");

        ErrorLabel.TextColor =
            GetColorResource(
                "DriveFleetDanger");

        ErrorLabel.Text =
            message;

        ErrorBorder.IsVisible =
            true;
    }

    /// <summary>
    /// Displays a successful authentication message.
    /// </summary>
    private void ShowSuccess(
        string message)
    {
        ErrorBorder.BackgroundColor =
            GetColorResource(
                "DriveFleetSuccessSoft");

        ErrorLabel.TextColor =
            GetColorResource(
                "DriveFleetSuccess");

        ErrorLabel.Text =
            message;

        ErrorBorder.IsVisible =
            true;
    }

    /// <summary>
    /// Clears the current validation or
    /// authentication message.
    /// </summary>
    private void HideMessage()
    {
        ErrorLabel.Text =
            string.Empty;

        ErrorBorder.IsVisible =
            false;
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