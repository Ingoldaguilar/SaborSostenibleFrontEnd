using System.Text.Json;
using System.Text;
using SaborSostenibleFrontEnd;
using SaborSostenibleFrontEnd.Request;

namespace SaborSostenibleFrontEnd;

public partial class RegisterPage : ContentPage
{
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

        ReqSignUp request = new ReqSignUp();
        request.Email = EmailEntry.Text;
        request.FirstName1 = NameEntry.Text;
        request.LatestName1 = LastNameEntry.Text;
        request.PasswordHash = PasswordEntry.Text;
        request.PhoneNumber = PhoneEntry.Text;

        using var client = new HttpClient();

        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://34.39.128.125/api/signUp", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Usuario registrado correctamente. Por favor verifique el correo", "OK");
                // Pasar el email al CodeVerificationPage
                await Navigation.PushAsync(new CodeVerificationPage(EmailEntry.Text));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"No se pudo registrar: {error}", "OK");
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

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}