using SaborSostenibleFrontEnd.Entities;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.VolunteerPages;

public partial class PendingDonationsPage : ContentPage
{
    private readonly ApiService _api = new ApiService();

    public PendingDonationsPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        Padding = new Thickness(0);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarDonaciones();
    }

    private async Task CargarDonaciones()
    {
        MostrarLoaderDonaciones(true);
        DonationsContainer.Children.Clear();

        try
        {
            var response = await _api.GetAsync<ResPendingDonations>("pendingDonationsVolunteer/get");

            if (response?.Success != true || response.PendingDonations == null)
            {
                DonationsContainer.Children.Add(new Label
                {
                    Text = "No hay donaciones pendientes.",
                    TextColor = Colors.Gray,
                    FontSize = 14,
                    HorizontalOptions = LayoutOptions.Center
                });
                return;
            }

            foreach (var d in response.PendingDonations)
            {
                var card = new Frame
                {
                    Padding = 15,
                    CornerRadius = 12,
                    BackgroundColor = Colors.White,
                    HasShadow = true,
                    BorderColor = Colors.Transparent
                };

                var stack = new VerticalStackLayout { Spacing = 8 };

                // Título Bolsa
                stack.Children.Add(new Label
                {
                    Text = $"\U0001F4E6 {d.BagDescription}",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16,
                    TextColor = Color.FromArgb("#789262")
                });

                // Restaurante
                stack.Children.Add(new BoxView { HeightRequest = 1, Color = Colors.LightGray });
                stack.Children.Add(new Label { Text = "Restaurante donante:", FontAttributes = FontAttributes.Bold, FontSize = 13 });
                stack.Children.Add(new Label { Text = d.BusinessName, FontSize = 13 });
                stack.Children.Add(new Label { Text = $"\U0001F4DE {d.BusinessPhone}", FontSize = 12 });
                stack.Children.Add(new Label { Text = $"\U0001F4CD {d.BusinessAddress}", FontSize = 12 });

                // Banco receptor
                stack.Children.Add(new BoxView { HeightRequest = 1, Color = Colors.LightGray, Margin = new Thickness(0, 8) });
                stack.Children.Add(new Label { Text = "Banco receptor:", FontAttributes = FontAttributes.Bold, FontSize = 13 });
                stack.Children.Add(new Label { Text = d.FoodBankName, FontSize = 13 });
                stack.Children.Add(new Label { Text = $"\U0001F4DE {d.FoodBankPhone}", FontSize = 12 });
                stack.Children.Add(new Label { Text = $"\U0001F4CD {d.FoodBankAddress}", FontSize = 12 });

                card.Content = stack;

                // Animación de entrada
                card.Opacity = 0;
                card.TranslationY = 30;

                DonationsContainer.Children.Add(card);

                // Fade + slide up
                await Task.WhenAll(
                    card.FadeTo(1, 250, Easing.CubicIn),
                    card.TranslateTo(0, 0, 250, Easing.CubicOut)
                );
            }
        }
        finally
        {
            MostrarLoaderDonaciones(false);
        }
    }

    private void MostrarLoaderDonaciones(bool mostrar)
    {
        DonationsLoader.IsVisible = mostrar;
        DonationsContainer.IsVisible = !mostrar;
    }


    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}
