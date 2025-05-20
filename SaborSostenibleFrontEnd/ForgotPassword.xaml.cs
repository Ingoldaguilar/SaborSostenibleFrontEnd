namespace SaborSostenibleFrontEnd;

public partial class ForgotPassword : ContentPage
{
	public ForgotPassword()
	{
		InitializeComponent();
	}

    private async void OnInicio_Tapped(object sender, TappedEventArgs e)
    {
		await Navigation.PopAsync();
    }

    private void OnOlvido_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Exito", "Contraseña restablecida exitosamente", "Cerrar");
    }
}