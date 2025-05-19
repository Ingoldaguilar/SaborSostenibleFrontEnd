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

    private void OnLogIn_Clicked(object sender, EventArgs e)
    {
        // hacer metodo para log In acá
    }
}