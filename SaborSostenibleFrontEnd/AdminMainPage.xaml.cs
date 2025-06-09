namespace SaborSostenibleFrontEnd;

public partial class AdminMainPage : TabbedPage
{
	public AdminMainPage()
	{
		InitializeComponent();
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

}