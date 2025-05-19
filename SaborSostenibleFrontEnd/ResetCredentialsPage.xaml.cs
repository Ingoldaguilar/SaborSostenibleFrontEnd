namespace SaborSostenibleFrontEnd;

public partial class ResetCredentialsPage : ContentPage
{
	public ResetCredentialsPage()
	{
		InitializeComponent();
	}

    private async void OnResetClicked(object sender, EventArgs e)
    {
        // Lógica para manejar el clic en el botón "Enviar"
        await DisplayAlert("Éxito", "Contraseña reestablecida", "OK");
    }
}