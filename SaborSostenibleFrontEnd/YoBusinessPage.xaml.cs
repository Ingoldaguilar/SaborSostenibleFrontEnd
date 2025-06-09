using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd;

public partial class YoBusinessPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();
    public YoBusinessPage()
	{
		InitializeComponent();
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