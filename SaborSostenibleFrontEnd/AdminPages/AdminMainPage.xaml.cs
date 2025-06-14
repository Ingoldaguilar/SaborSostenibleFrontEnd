using SaborSostenibleFrontEnd.Security;
using SaborSostenibleFrontEnd.Response;

namespace SaborSostenibleFrontEnd.AdminPages;

public partial class AdminMainPage : TabbedPage
{
    private readonly ApiService _apiService = new ApiService();
    public AdminMainPage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var resp = await _apiService.GetAsync<ResGreetingInfo>("userGreetingInfo/get");
        if (resp?.Success == true && resp.GreetingInfo != null)
        {
            LabelFullName.Text = resp.GreetingInfo.FullName.Trim();
            LabelEmail.Text = resp.GreetingInfo.Email;
        }
    }

    private async void OnAgregarRestauranteTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new InsertBusinessPage());
    }
    
    private async void OnAgregarBancoTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new InsertFoodBankPage());
    }

    private async void OnSolicitudesTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListVolunteerRequestsPage());
    }

    private async void OnEstadosBancosTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListFoodBanksPage());
    }

    private async void OnEstadosRestaurantesTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListBusinessesPage());
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // Limpia la sesión actual
        await _apiService.Logout();

    }

}