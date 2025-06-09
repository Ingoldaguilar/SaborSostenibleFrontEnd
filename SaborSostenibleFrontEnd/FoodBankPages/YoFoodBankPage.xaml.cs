using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.FoodBankPages;

public partial class YoFoodBankPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();
    public YoFoodBankPage()
	{
		InitializeComponent();
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UpdateFoodBankPage());
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // Limpia la sesión actual
        await _apiService.Logout();

    }

}