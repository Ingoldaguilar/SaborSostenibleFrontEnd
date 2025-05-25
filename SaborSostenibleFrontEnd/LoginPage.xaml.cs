using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using System.Text.Json;
using System.Text;

namespace SaborSostenibleFrontEnd;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnRegister_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    private async void OnLogIn_Clicked(object sender, EventArgs e)
    {
        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            await DisplayAlert("Error", "Por favor ingrese su correo electrónico", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Error", "Por favor ingrese su contraseña", "OK");
            return;
        }

        // Validación básica de formato de email
        if (!IsValidEmail(EmailEntry.Text))
        {
            await DisplayAlert("Error", "Por favor ingrese un correo electrónico válido", "OK");
            return;
        }

        // Crear el request
        ReqLogin request = new ReqLogin
        {
            Email = EmailEntry.Text.Trim(),
            Password = PasswordEntry.Text
        };

        using var client = new HttpClient();

        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://34.39.128.125/api/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<ResLogin>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse?.Success == true)
                {
                    // Login exitoso
                    if (!string.IsNullOrEmpty(loginResponse.SessionId))
                    {
                        // Se guarda el token en preferences
                        Preferences.Set("SessionId", loginResponse.SessionId);
                        Preferences.Set("UserEmail", EmailEntry.Text.Trim());

                        await DisplayAlert("Éxito", "Inicio de sesión exitoso", "OK");

                        // Navegar a la página principal de la aplicación
                        Application.Current.MainPage = new MainPage(); 
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo obtener la sesión", "OK");
                    }
                }
                else
                {
                    // Mostrar errores específicos del servidor
                    var errorMessages = loginResponse?.Errors?.Select(e => e.Description) ?? new[] { "Error desconocido" };
                    var errorText = string.Join(", ", errorMessages);
                    await DisplayAlert("Error", $"Login fallido: {errorText}", "OK");
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"No se pudo iniciar sesión: {error}", "OK");
            }
        }
        catch (HttpRequestException httpEx)
        {
            await DisplayAlert("Error", $"Error de conexión: {httpEx.Message}", "OK");
        }
        catch (JsonException jsonEx)
        {
            await DisplayAlert("Error", $"Error al procesar la respuesta: {jsonEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
        }
    }

    private async void OnOlvido_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new ForgotPassword());
    }

    // Método auxiliar para validar formato de email
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
}