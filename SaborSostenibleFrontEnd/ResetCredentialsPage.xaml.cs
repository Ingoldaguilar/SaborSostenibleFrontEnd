using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd;

public partial class ResetCredentialsPage : ContentPage
{
    private readonly ApiService _api = new ApiService();
    private readonly string _email;

    public ResetCredentialsPage(string correo)
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        Padding = new Thickness(0);
        _email = correo;
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        var codigo = entryCodigo.Text?.Trim();
        var password1 = contra1.Text?.Trim();
        var password2 = contra2.Text?.Trim();

        if (string.IsNullOrWhiteSpace(codigo) ||
            string.IsNullOrWhiteSpace(password1) ||
            string.IsNullOrWhiteSpace(password2))
        {
            await DisplayAlert("Error", "Todos los campos son obligatorios.", "Cerrar");
            return;
        }

        if (password1 != password2)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden.", "Cerrar");
            return;
        }

        var loader = new LoadingPage();
        await Navigation.PushModalAsync(loader);

        ResResetPassword res = null;

        try
        {
            var req = new ReqResetPassword
            {
                Email = _email,
                PasswordHash1 = password1,
                PasswordHash2 = password2,
                VerificationCode = codigo
            };

            res = await _api.PostAsync<ReqResetPassword, ResResetPassword>(
                "updateUserPasswordHash/post", req);
        }
        finally
        {
            await Navigation.PopModalAsync(); // Se ejecuta siempre, éxito o error
        }

        if (res?.Success == true)
        {
            await DisplayAlert("Éxito", "Tu contraseña ha sido restablecida.", "Cerrar");

            // Ir directamente al login, limpiando la navegación
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
        else
        {
            var errores = res?.Errors?.Select(e => e.Description)
                           ?? new[] { "Ha ocurrido un error inesperado." };
            await DisplayAlert("Error", string.Join("\n", errores), "Cerrar");
        }
    }

    private async void OnOlvido_Clicked(object sender, EventArgs e)
    {
        var correo = _email;

        if (string.IsNullOrWhiteSpace(correo))
        {
            await DisplayAlert("Error", "Por favor ingresa un correo válido.", "Cerrar");
            return;
        }

        var loader = new LoadingPage();
        await Navigation.PushModalAsync(loader);

        var req = new GenerateNewVerificationCodeRequest
        {
            Email = correo
        };

        var res = await _api.PostAsync<GenerateNewVerificationCodeRequest, GenerateNewVerificationCodeResponse>(
            "generateNewVerificationCode/post", req);

        await Navigation.PopModalAsync();

        if (res?.Success == true)
        {
            await DisplayAlert("Éxito", "Se ha enviado un código de verificación. Revisa tu correo.", "Cerrar");
        }
        else
        {
            var errores = res?.Errors?.Select(e => e.Description) ?? new[] { "Ha ocurrido un error inesperado." };
            await DisplayAlert("Error", string.Join("\n", errores), "Cerrar");
        }
    }

}