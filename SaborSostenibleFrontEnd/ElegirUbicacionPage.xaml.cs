using System.Net.NetworkInformation;

namespace SaborSostenibleFrontEnd;

public partial class ElegirUbicacionPage : ContentPage
{
    private readonly Action<Location> onUbicacionConfirmada;
    double selectedLat = 0;
    double selectedLng = 0;

    public ElegirUbicacionPage(Action<Location> onUbicacionConfirmada)
    {
        InitializeComponent();
        this.onUbicacionConfirmada = onUbicacionConfirmada;
        LoadMapAsync();
    }

    private async void LoadMapAsync()
    {
        double centerLat = 0;
        double centerLng = 0;
        int zoom = 2;

        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync()
                           ?? await Geolocation.GetLocationAsync();

            if (location != null)
            {
                centerLat = location.Latitude;
                centerLng = location.Longitude;
                zoom = 15;
            }
        }
        catch
        {
            // Si falla, usamos coordenadas por defecto (0,0)
        }

        string html = $@"
        <!DOCTYPE html>
        <html>
        <head>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.3/dist/leaflet.css'/>
        <script src='https://unpkg.com/leaflet@1.9.3/dist/leaflet.js'></script>
        <style>
          #map {{ height: 100vh; width: 100vw; margin: 0; padding: 0; }}
        </style>
        </head>
        <body>
        <div id='map'></div>
        <script>
          var map = L.map('map').setView([{centerLat}, {centerLng}], {zoom});
          L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
              maxZoom: 19
          }}).addTo(map);

          var marker;

          function onMapClick(e) {{
            if (marker) {{
              map.removeLayer(marker);
            }}
            marker = L.marker(e.latlng).addTo(map);
            window.location.href = 'callback://' + e.latlng.lat + '/' + e.latlng.lng;
          }}

          map.on('click', onMapClick);
        </script>
        </body>
        </html>";

        mapWebView.Source = new HtmlWebViewSource { Html = html };
        mapWebView.Navigating += MapWebView_Navigating;
    }

    private void MapWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("callback://"))
        {
            e.Cancel = true;
            var parts = e.Url.Replace("callback://", "").Split('/');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out double lat) &&
                double.TryParse(parts[1], out double lng))
            {
                selectedLat = lat;
                selectedLng = lng;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UbicacionLabel.Text = $"Ubicación seleccionada: Lat {lat:F5}, Lng {lng:F5}";
                });
            }
        }
    }

    private async void OnConfirmarClicked(object sender, EventArgs e)
    {
        if (selectedLat == 0 && selectedLng == 0)
        {
            await DisplayAlert("Error", "Debes seleccionar una ubicación tocando el mapa.", "OK");
            return;
        }

        onUbicacionConfirmada?.Invoke(new Location(selectedLat, selectedLng));
        await Navigation.PopAsync(); // volver a RegisterPage
    }
}