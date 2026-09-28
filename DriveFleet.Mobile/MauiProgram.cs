using DriveFleet.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace DriveFleet.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = 
                MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton(
                new HttpClient
                {
                    BaseAddress =
                        new Uri(
                            ApiSettings.BaseUrl),

                    Timeout =
                        TimeSpan.FromSeconds(30)
                });

            builder.Services.AddSingleton<AuthApiService>();

            builder.Services.AddSingleton<SessionService>();

#if DEBUG
            builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}
