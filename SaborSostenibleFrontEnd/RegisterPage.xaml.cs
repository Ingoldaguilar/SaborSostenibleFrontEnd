using SaborSostenibleFrontEnd.Request;
using System.Text;
using System.Text.Json;
using System.Net.Mail;
using Microsoft.Maui.ApplicationModel;

namespace SaborSostenibleFrontEnd;

public partial class RegisterPage : ContentPage
{
    private static readonly HttpClient httpClient = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private decimal? latitude = null;
    private decimal? longitude = null;

    public RegisterPage()
    {
        InitializeComponent();

        NavigationPage.SetHasNavigationBar(this, false);
        Padding = new Thickness(0);
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (!ValidarCampos())
            return;

        await MostrarLoading(async () =>
        {
            await RealizarRegistroAsync();
        });
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(LastNameEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(RepeatPasswordEntry.Text))
        {
            MostrarError("Complete los campos obligatorios");
            return false;
        }

        if (!EsEmailValido(EmailEntry.Text))
        {
            MostrarError("Correo electrónico no válido");
            return false;
        }

        if (PasswordEntry.Text != RepeatPasswordEntry.Text)
        {
            MostrarError("Las contraseñas no coinciden");
            return false;
        }

        if (PasswordEntry.Text.Length < 6)
        {
            MostrarError("La contraseña debe tener al menos 6 caracteres");
            return false;
        }

        return true;
    }

    private bool EsEmailValido(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private async void MostrarError(string mensaje)
    {
        await DisplayAlert("Error", mensaje, "OK");
    }

    private async Task MostrarLoading(Func<Task> accion)
    {
        var loadingPage = new LoadingPage();
        await Navigation.PushModalAsync(loadingPage);
        try
        {
            await accion.Invoke();
        }
        finally
        {
            await Navigation.PopModalAsync();
        }
    }

    //private async void OnObtenerUbicacionClicked(object sender, EventArgs e)
    //{
    //    try
    //    {
    //        var location = await Geolocation.GetLastKnownLocationAsync();

    //        if (location == null)
    //        {
    //            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
    //            location = await Geolocation.GetLocationAsync(request);
    //        }

    //        if (location != null)
    //        {
    //            latitude = (decimal)location.Latitude;
    //            longitude = (decimal)location.Longitude;
    //            UbicacionLabel.Text = $"Ubicación detectada: {latitude:F5}, {longitude:F5}";
    //        }
    //        else
    //        {
    //            UbicacionLabel.Text = "No se pudo obtener la ubicación.";
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        UbicacionLabel.Text = "Error al obtener ubicación.";
    //        await DisplayAlert("Error", $"No se pudo obtener la ubicación: {ex.Message}", "OK");
    //    }
    //}

    private async void OnElegirUbicacionClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ElegirUbicacionPage((ubicacion) =>
        {
            latitude = (decimal)ubicacion.Latitude;
            longitude = (decimal)ubicacion.Longitude;

            // Actualizar la UI desde el hilo principal
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UbicacionLabel.Text = $"Ubicación elegida: {latitude:F5}, {longitude:F5}";
            });
        }));
    }

    private async Task RealizarRegistroAsync()
    {
        try
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                MostrarError("No hay conexión a internet");
                return;
            }

            var request = new ReqSignUp
            {
                FirstName1 = NameEntry.Text.Trim(),
                FirstName2 = SecondNameEntry.Text?.Trim() ?? "",
                LatestName1 = LastNameEntry.Text.Trim(),
                LastName2 = SecondLastNameEntry.Text?.Trim() ?? "",
                PhoneNumber = PhoneEntry.Text?.Trim(),
                Address = AddressEntry.Text?.Trim(),
                Email = EmailEntry.Text.Trim(),
                PasswordHash = PasswordEntry.Text,
                Latitude = latitude,
                Longitude = longitude
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "SaborSostenible-Mobile/1.0");

            var response = await httpClient.PostAsync("http://34.39.128.125/api/signUp", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Usuario registrado. Revise su correo", "OK");
                await Navigation.PushAsync(new CodeVerificationPage(EmailEntry.Text));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MostrarError($"Error al registrar: {response.StatusCode} - {error}");
            }
        }
        catch (Exception ex)
        {
            MostrarError($"Error inesperado: {ex.Message}");
        }
    }
}
