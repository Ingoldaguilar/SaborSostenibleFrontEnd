using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.FoodBankPages
{
    public partial class ReceivedBagsHistoryPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private const string _baseUrl = "http://34.39.128.125/";

        public ReceivedBagsHistoryPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            HistoryContainer.Children.Clear();

            var resp = await _api.GetAsync<ResReceivedBagsHistory>("receivedBagHistoryByFoodBank/get");
            if (resp == null)
            {
                await DisplayAlert("Error", "No se pudo conectar al servidor.", "OK");
                return;
            }

            if (!resp.Success || resp.ReceivedBagsHistory?.Any() != true)
            {
                HistoryContainer.Children.Add(new Label
                {
                    Text = "Actualmente no hay historial de bolsas recibidas.",
                    FontSize = 16,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                });
                return;
            }

            foreach (var item in resp.ReceivedBagsHistory)
            {
                // Card
                var frame = new Frame
                {
                    CornerRadius = 10,
                    Padding = 0,
                    BackgroundColor = Colors.White,
                    HasShadow = true
                };

                var layout = new VerticalStackLayout { Spacing = 0 };

                // 1) Encabezado: logo + nombre
                var header = new HorizontalStackLayout
                {
                    Spacing = 10,
                    Padding = new Thickness(12, 10),
                    VerticalOptions = LayoutOptions.Center
                };
                header.Children.Add(new Image
                {
                    Source = ImageSource.FromUri(new Uri(_baseUrl + item.LogoUrl)),
                    WidthRequest = 40,
                    HeightRequest = 40,
                    Aspect = Aspect.AspectFill
                });
                header.Children.Add(new Label
                {
                    Text = item.BusinessName,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16,
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center
                });
                layout.Children.Add(header);

                // 2) Descripción y fecha
                var descSection = new HorizontalStackLayout
                {
                    Spacing = 6,
                    Padding = new Thickness(12, 0),
                    VerticalOptions = LayoutOptions.Center
                };
                // icono calendario (reemplazar “calendar.png” por tu recurso)
                descSection.Children.Add(new Image
                {
                    Source = "calendar.png",
                    WidthRequest = 16,
                    HeightRequest = 16,
                    Aspect = Aspect.AspectFit
                });
                descSection.Children.Add(new Label
                {
                    Text = item.DonationDate.ToLocalTime().ToString("dd/MM/yyyy"),
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                // bolsa sorpresa
                descSection.Children.Add(new Label
                {
                    Text = item.BagDescription,
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                layout.Children.Add(descSection);

                // 3) Estado en barra inferior
                layout.Children.Add(new BoxView
                {
                    Color = Colors.Green,
                    HeightRequest = 4,
                    HorizontalOptions = LayoutOptions.Fill
                });
                layout.Children.Add(new Label
                {
                    Text = item.State,
                    BackgroundColor = Colors.Green,
                    TextColor = Colors.White,
                    FontSize = 14,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    HeightRequest = 28,
                    Margin = new Thickness(0, 0, 0, -4)
                });

                frame.Content = layout;
                HistoryContainer.Children.Add(frame);
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}
