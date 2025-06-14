using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd;

public partial class ForgotPassword : ContentPage
{
    private readonly ApiService _api = new ApiService();
    public ForgotPassword()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        Padding = new Thickness(0);
    }

    private async void OnInicio_Tapped(object sender, TappedEventArgs e)
    {
		await Navigation.PopAsync();
    }

    private async void OnOlvido_Clicked(object sender, EventArgs e)
    {
        var correo = entryCorreo.Text?.Trim();

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
            await Navigation.PushAsync(new ResetCredentialsPage(correo));
        }
        else
        {
            var errores = res?.Errors?.Select(e => e.Description) ?? new[] { "Ha ocurrido un error inesperado." };
            await DisplayAlert("Error", string.Join("\n", errores), "Cerrar");
        }
    }
}