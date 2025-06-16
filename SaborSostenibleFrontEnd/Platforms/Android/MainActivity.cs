using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;
using Android.Views;

namespace SaborSostenibleFrontEnd;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
          base.OnCreate(savedInstanceState);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        {
            // Cambia este color por el verde exacto de tu app
            Window?.SetStatusBarColor(Android.Graphics.Color.ParseColor("#789262"));
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                var decorView = Window?.DecorView;
                if (decorView != null)
                {
                    decorView.SystemUiVisibility = (StatusBarVisibility)(
                        SystemUiFlags.LayoutStable |
                        SystemUiFlags.LightStatusBar  // Para íconos oscuros sobre fondo verde claro
                    );
                }
            }
        }
       
    }
}