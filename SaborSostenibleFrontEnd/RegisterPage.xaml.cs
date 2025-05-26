using System.Text.Json;
using System.Text;
using SaborSostenibleFrontEnd;
using SaborSostenibleFrontEnd.Request;

namespace SaborSostenibleFrontEnd;

public partial class RegisterPage : ContentPage
{
    private static readonly HttpClient httpClient = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(30) // Timeout de 30 segundos
    };

    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(LastNameEntry.Text) ||
            string.IsNullOrWhiteSpace(PhoneEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(RepeatPasswordEntry.Text))
        {
            await DisplayAlert("Error", "Todos los campos son obligatorios", "OK");
            return;
        }

        if (PasswordEntry.Text != RepeatPasswordEntry.Text)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
            return;
        }

        // Validación adicional del email
        if (!IsValidEmail(EmailEntry.Text))
        {
            await DisplayAlert("Error", "Por favor ingrese un email válido", "OK");
            return;
        }

        // Mostrar indicador de carga
        //LoadingIndicator.IsVisible = true;
        //RegisterButton.IsEnabled = false;

        try
        {
            // Verificar conectividad de red
            var connectivity = Connectivity.Current;
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Error", "No hay conexión a internet. Verifique su conexión y vuelva a intentar.", "OK");
                return;
            }

            // Probar conectividad con el servidor
            await TestServerConnectivity();

            ReqSignUp request = new ReqSignUp
            {
                Email = EmailEntry.Text.Trim(),
                FirstName1 = NameEntry.Text.Trim(),
                LatestName1 = LastNameEntry.Text.Trim(),
                PasswordHash = PasswordEntry.Text,
                PhoneNumber = PhoneEntry.Text.Trim()
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            System.Diagnostics.Debug.WriteLine($"JSON enviado: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Agregar headers adicionales
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "SaborSostenible-Mobile/1.0");

            var response = await httpClient.PostAsync("http://34.39.128.125/api/signUp", content);

            System.Diagnostics.Debug.WriteLine($"Status Code: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Response Headers: {response.Headers}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Usuario registrado correctamente. Por favor verifique el correo", "OK");
                await Navigation.PushAsync(new CodeVerificationPage(EmailEntry.Text));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error response: {error}");
                await DisplayAlert("Error", $"No se pudo registrar: {response.StatusCode} - {error}", "OK");
            }
        }
        catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException)
        {
            await DisplayAlert("Error", "La solicitud tardó demasiado tiempo. Verifique su conexión a internet e intente nuevamente.", "OK");
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HttpRequestException: {httpEx}");
            string errorMessage = GetFriendlyErrorMessage(httpEx);
            await DisplayAlert("Error de Conexión", errorMessage, "OK");
        }
        catch (JsonException jsonEx)
        {
            System.Diagnostics.Debug.WriteLine($"JsonException: {jsonEx}");
            await DisplayAlert("Error", $"Error al procesar los datos: {jsonEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
            await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
        }
        //finally
        //{
        //    LoadingIndicator.IsVisible = false;
        //    RegisterButton.IsEnabled = true;
        //}
    }

    private async Task TestServerConnectivity()
    {
        try
        {
            var testRequest = new HttpRequestMessage(HttpMethod.Head, "http://34.39.128.125/api/health");
            var testResponse = await httpClient.SendAsync(testRequest);
            System.Diagnostics.Debug.WriteLine($"Server connectivity test: {testResponse.StatusCode}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Server connectivity test failed: {ex.Message}");
            throw new HttpRequestException("No se puede conectar con el servidor. Verifique que el servidor esté disponible.", ex);
        }
    }

    private string GetFriendlyErrorMessage(HttpRequestException httpEx)
    {
        if (httpEx.Message.Contains("Name or service not known") ||
            httpEx.Message.Contains("nodename nor servname provided"))
        {
            return "No se puede resolver la dirección del servidor. Verifique su conexión a internet.";
        }

        if (httpEx.Message.Contains("Connection refused") ||
            httpEx.Message.Contains("No connection could be made"))
        {
            return "El servidor no está disponible en este momento. Intente nuevamente más tarde.";
        }

        if (httpEx.Message.Contains("timeout"))
        {
            return "La conexión tardó demasiado tiempo. Verifique su conexión a internet.";
        }

        if (httpEx.Message.Contains("SSL") || httpEx.Message.Contains("certificate"))
        {
            return "Error de seguridad en la conexión. Contacte al administrador.";
        }

        return $"Error de conexión: {httpEx.Message}\n\nSugerencias:\n• Verifique su conexión a internet\n• Asegúrese de que el servidor esté disponible\n• Intente nuevamente en unos momentos";
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}