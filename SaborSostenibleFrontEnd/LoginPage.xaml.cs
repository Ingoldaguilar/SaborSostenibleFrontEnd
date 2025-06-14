using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using System.Text.Json;
using System.Text;
using System.Net.Mail;
using SaborSostenibleFrontEnd.FoodBankPages;
using SaborSostenibleFrontEnd.AdminPages;
using SaborSostenibleFrontEnd.BusinessPages;

namespace SaborSostenibleFrontEnd;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void OnRegister_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    private async void OnLogIn_Clicked(object sender, EventArgs e)
    {
        if (!ValidarCampos())
            return;

        await MostrarLoading(async () =>
        {
            await RealizarLoginAsync();
        });
    }

    private async void OnOlvido_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new ForgotPassword());
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            MostrarError("Por favor ingrese su correo electrónico");
            return false;
        }

        if (!EsEmailValido(EmailEntry.Text))
        {
            MostrarError("Correo electrónico no válido");
            return false;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            MostrarError("Por favor ingrese su contraseña");
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

    private async Task RealizarLoginAsync()
    {
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
                var loginResponse = JsonSerializer.Deserialize<ResLogin>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (loginResponse?.Success == true && !string.IsNullOrEmpty(loginResponse.SessionId))
                {
                    Preferences.Set("SessionId", loginResponse.SessionId);
                    Console.WriteLine($"SessionId guardado: {loginResponse.SessionId}");
                    Preferences.Set("UserEmail", EmailEntry.Text.Trim());
                    Preferences.Set("UserName", EmailEntry.Text.Trim());
                    Preferences.Set("UserRole", loginResponse.UserRole);

                    Page targetPage;

                    switch (loginResponse.UserRole)
                    {
                        case "Customer":
                            targetPage = new MainPage();
                            break;
                        case "Admin":
                            targetPage = new AdminMainPage();
                            break;
                        case "Business":
                            targetPage = new BusinessMainPage();
                            break;
                        case "FoodBank":
                            targetPage = new FoodBankMainPage();
                            break;
                        case "Volunteer":
                            targetPage = new MainPage();
                            break;
                        default:
                            MostrarError("Rol de usuario no reconocido.");
                            return;
                    }

                    NavigationPage.SetHasNavigationBar(targetPage, false);
                    Application.Current.MainPage = new NavigationPage(targetPage);


                }
                else
                {
                    var errorMessages = loginResponse?.Errors?.Select(e => e.Description) ?? new[] { "Error desconocido" };
                    MostrarError(string.Join(", ", errorMessages));
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MostrarError($"No se pudo iniciar sesión: {error}");
            }
        }
        catch (Exception ex)
        {
            MostrarError($"Error inesperado: {ex.Message}");
        }
    }
}
