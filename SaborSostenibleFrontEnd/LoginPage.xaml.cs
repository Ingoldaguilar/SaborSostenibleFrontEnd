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
        // Validaciones b�sicas
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            await DisplayAlert("Error", "Por favor ingrese su correo electr�nico", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Error", "Por favor ingrese su contrase�a", "OK");
            return;
        }

        // Validaci�n b�sica de formato de email
        if (!IsValidEmail(EmailEntry.Text))
        {
            await DisplayAlert("Error", "Por favor ingrese un correo electr�nico v�lido", "OK");
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

                        await DisplayAlert("�xito", "Inicio de sesi�n exitoso", "OK");

                        // Navegar a la p�gina principal de la aplicaci�n
                        Application.Current.MainPage = new MainPage(); 
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo obtener la sesi�n", "OK");
                    }
                }
                else
                {
                    // Mostrar errores espec�ficos del servidor
                    var errorMessages = loginResponse?.Errors?.Select(e => e.Description) ?? new[] { "Error desconocido" };
                    var errorText = string.Join(", ", errorMessages);
                    await DisplayAlert("Error", $"Login fallido: {errorText}", "OK");
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"No se pudo iniciar sesi�n: {error}", "OK");
            }
        }
        catch (HttpRequestException httpEx)
        {
            await DisplayAlert("Error", $"Error de conexi�n: {httpEx.Message}", "OK");
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

    // M�todo auxiliar para validar formato de email
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