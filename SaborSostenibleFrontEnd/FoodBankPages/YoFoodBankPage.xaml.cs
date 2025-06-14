using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.FoodBankPages;

public partial class YoFoodBankPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();
    public YoFoodBankPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override async void OnAppearing()
    {
        var response = await _apiService.GetAsync<ResFoodBankNameAndEmail>("foodBankNameAndEmail/get");

        if (response != null && response.Success)
        {
            LabelName.Text = response.Name;
            LabelEmail.Text = response.Email;
        }
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