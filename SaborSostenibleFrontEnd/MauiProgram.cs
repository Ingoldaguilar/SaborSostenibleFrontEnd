using Microsoft.Extensions.Logging;

namespace SaborSostenibleFrontEnd;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("Poppins-Regular.ttf", "Poppins");
                fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
            });

        builder.Services.AddSingleton<MainPage>();

#if ANDROID
        // Configuración específica para Android
        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {
#if ANDROID
            var activity = handler.PlatformView as AndroidX.AppCompat.App.AppCompatActivity;
            if (activity?.Window != null)
            {
                activity.Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
            }
#endif
        });
#endif

        return builder.Build();
    }
}