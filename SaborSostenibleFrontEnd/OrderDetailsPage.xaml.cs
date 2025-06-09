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
    public partial class OrderDetailsPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private readonly int _orderId;
        private bool _showAll = false;

        public OrderDetailsPage(int orderId)
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

            var list = resp.OrderDetailsWithBagDetails
                .Take(_showAll ? int.MaxValue : 3);

            foreach (var bag in list)
            {
                var frame = new Frame
                {
                    CornerRadius = 10,
                    Padding = 0,
                    BackgroundColor = Colors.White,
                    HasShadow = true
                };

                var hl = new HorizontalStackLayout
                {
                    Spacing = 10,
                    Padding = new Thickness(12, 10),
                    VerticalOptions = LayoutOptions.Center
                };

                // Imagen genérica de Bolsa sorpresa
                hl.Children.Add(new Image
                {
                    Source = ImageSource.FromUri(new Uri("http://34.39.128.125/Uploads/Logos/bolsaSorpresaEstandar.jpg")),
                    WidthRequest = 60,
                    HeightRequest = 60,
                    Aspect = Aspect.AspectFill
                });

                var info = new VerticalStackLayout { Spacing = 4 };
                info.Children.Add(new Label
                {
                    Text = bag.BagDescription,
                    FontSize = 16,
                    TextColor = Colors.Black
                });
                info.Children.Add(new Label
                {
                    Text = $"{bag.Price} colones",
                    FontSize = 14,
                    TextColor = Colors.Gray
                });

                hl.Children.Add(info);
                frame.Content = hl;
                DetailsContainer.Children.Add(frame);
            }

            // Ajusta texto del botón Ver más/menos
            ((Button)FindByName("OnShowMoreClicked")).Text =
                _showAll ? "Ver menos ?" : "Ver más ?";
        }

        private async void OnShowMoreClicked(object sender, EventArgs e)
        {
            _showAll = !_showAll;
            await LoadOrderDetailsAsync();
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}
