namespace SaborSostenibleFrontEnd;

public partial class LoadingPage : ContentPage
{
	public LoadingPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        Padding = new Thickness(0);
    }
}