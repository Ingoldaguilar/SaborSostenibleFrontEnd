using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd
{
    public partial class BusinessOrderDetailsPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private readonly int _orderId;

        public BusinessOrderDetailsPage(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadOrderDetailsAsync();
        }

        private async Task LoadOrderDetailsAsync()
        {
            DetailsContainer.Children.Clear();

            var req = new ReqOrderDetailsWithBagDetails { OrderId = _orderId };
            var resp = await _api.PostAsync<ReqOrderDetailsWithBagDetails, ResOrderDetailsWithBagDetails>(
                "orderDetailsWithBagDetails/post", req);

            if (resp == null || !resp.Success || resp.OrderDetailsWithBagDetails == null)
            {
                await DisplayAlert("Error", "No se recuperaron detalles.", "OK");
                return;
            }

            var list = resp.OrderDetailsWithBagDetails;

            foreach (var bag in list)
            {
                var grid = new Grid
                {
                    Padding = new Thickness(12),
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(56) }, // Imagen fija
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // Bolsa
                        new ColumnDefinition { Width = new GridLength(80) } // Precio
                    },
                    VerticalOptions = LayoutOptions.Center
                };

                // Imagen
                var logoFrame = new Frame
                {
                    CornerRadius = 28,
                    WidthRequest = 56,
                    HeightRequest = 56,
                    Padding = 0,
                    BackgroundColor = Colors.Gray,
                    IsClippedToBounds = true,
                    HasShadow = false,
                    Content = new Image
                    {
                        Source = ImageSource.FromUri(new Uri("http://34.39.128.125/Uploads/Logos/bolsaSorpresaEstandar.jpg")),
                        Aspect = Aspect.AspectFill
                    }
                };
                grid.Add(logoFrame, 0, 0);

                // Descripción
                grid.Add(new Label
                {
                    Text = bag.BagDescription,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(8, 0, 0, 0),
                    LineBreakMode = LineBreakMode.WordWrap,
                }, 1, 0);

                // Precio
                grid.Add(new Label
                {
                    Text = $"\u20A1{bag.Price:N0}",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#2E7D32"),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.End
                }, 2, 0);

                // Card
                var frame = new Frame
                {
                    CornerRadius = 6,
                    Padding = 0,
                    BackgroundColor = Colors.White,
                    HasShadow = true,
                    BorderColor = Colors.Transparent,
                    Margin = new Thickness(0, 0, 0, 4),
                    Content = grid
                };

                DetailsContainer.Children.Add(frame);
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}