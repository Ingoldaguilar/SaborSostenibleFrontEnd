namespace SaborSostenibleFrontEnd;

public partial class ResetCredentialsPage : ContentPage
{
	public ResetCredentialsPage()
	{
		InitializeComponent();
	}

    private async void OnResetClicked(object sender, EventArgs e)
    {
        // L�gica para manejar el clic en el bot�n "Enviar"
        await DisplayAlert("�xito", "Contrase�a reestablecida", "OK");
    }
}