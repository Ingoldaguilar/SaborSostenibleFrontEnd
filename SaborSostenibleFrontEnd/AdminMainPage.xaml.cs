using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd;

public partial class AdminMainPage : TabbedPage
{
    private readonly ApiService _apiService = new ApiService();
    public AdminMainPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var email = Preferences.Get("UserEmail", "sin-correo@sabor.com");
        LabelEmail.Text = email;
    }

    private async void OnUsuariosTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new InsertBusinessPage());
    }

    private async void OnRestaurantesTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new InsertFoodBankPage());
    }

    private async void OnReportesTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListVolunteerRequestsPage());
    }

    private async void OnEstadisticasTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListFoodBanksPage());
    }

    private async void OnConfiguracionTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListBusinessesPage());
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // Limpia la sesión actual
        await _apiService.Logout();

    }

}