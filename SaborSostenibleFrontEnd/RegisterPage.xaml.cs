using System.Text.Json;
using System.Text;
using SaborSostenibleFrontEnd;

namespace SaborSostenibleFrontEnd;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent(); // Esto inicializa los elementos definidos en XAML
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (PasswordEntry.Text != RepeatPasswordEntry.Text)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
            return;
        }

        var user = new
        {
            Name = NameEntry.Text,
            Phone = PhoneEntry.Text,
            Email = EmailEntry.Text,
            Password = PasswordEntry.Text
        };

        using var client = new HttpClient();
        try
        {
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:44313/api/signUp", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");
                await Navigation.PushAsync(new LoginPage());
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
