using Microsoft.Maui.Controls;

namespace SaborSostenibleFrontEnd
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var loginPage = new AdminMainPage();
            NavigationPage.SetHasNavigationBar(loginPage, false);

            MainPage = new NavigationPage(loginPage)
            {
                BarBackgroundColor = Colors.Transparent,
                BarTextColor = Colors.Transparent
            };
        }
    }
}
