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
            var response = await client.PostAsync("https://localhost:44313/api/signUp", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Usuario registrado correctamente. Porfavor verifique el correo", "OK");
                await Navigation.PushAsync(new CodeVerificationPage());
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"No se pudo registrar: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error de conexión: {ex.Message}", "OK");
        }
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
