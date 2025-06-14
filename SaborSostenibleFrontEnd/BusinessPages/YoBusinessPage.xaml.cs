using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.BusinessPages;

public partial class YoBusinessPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();
    public YoBusinessPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override async void OnAppearing()
    {
        var response = await _apiService.GetAsync<ResBusinessNameAndEmail>("businessNameAndEmail/get");

        if (response != null && response.Success)
        {
            LabelName.Text = response.BusinessName;
            LabelEmail.Text = response.Email;
        }
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UpdateBusinessPage());
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // Limpia la sesión actual
        await _apiService.Logout();

    }

    private async void OnBagsHistoryClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SurpriseBagsForBusinessPage());

    }

    
}