using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;
using System.Net.Http;
using System.Text.Json;

namespace SaborSostenibleFrontEnd
{
    public partial class OrderDetailsPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private readonly int _orderId;

        public OrderDetailsPage(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            NavigationPage.SetHasNavigationBar(this, false);
            Padding = new Thickness(0);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadOrderDetailsAsync();
            _ = LoadOrderInfoAsync();
        }

        private async Task LoadOrderInfoAsync()
        {
            try
            {
                var token = Preferences.Get("SessionId", null);
                if (string.IsNullOrEmpty(token)) return;

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync("http://34.39.128.125/api/userOrders/get");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("OrdersByUserId", out var ordenes))
                    {
                        foreach (var item in ordenes.EnumerateArray())
                        {
                            if (item.GetProperty("OrderId").GetInt32() == _orderId)
                            {
                                RestaurantNameLabel.Text = item.GetProperty("BusinessName").GetString();
                                OrderDateLabel.Text = DateTime.Parse(item.GetProperty("OrderDate").GetString()).ToString("dd/MM/yyyy HH:mm");
                                OrderStatusLabel.Text = item.GetProperty("State").GetString();
                                TotalLabel.Text = $"Total: ₡{item.GetProperty("TotalAmount").GetDecimal():N2}";
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar información del pedido: {ex.Message}", "OK");
            }
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
                    Padding = new Thickness(15),
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(60) }, // Imagen
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // Descripción
                        new ColumnDefinition { Width = new GridLength(80) } // Precio
                    },
                    VerticalOptions = LayoutOptions.Center
                };

                // Imagen
                var logoFrame = new Frame
                {
                    CornerRadius = 30,
                    WidthRequest = 60,
                    HeightRequest = 60,
                    Padding = 0,
                    BackgroundColor = Colors.LightGray,
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
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(12, 0, 0, 0),
                    LineBreakMode = LineBreakMode.WordWrap,
                }, 1, 0);

                // Precio
                grid.Add(new Label
                {
                    Text = $"₡{bag.Price:N0}",
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#2E7D32"),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.End
                }, 2, 0);

                // Card
                var frame = new Frame
                {
                    CornerRadius = 12,
                    Padding = 0,
                    BackgroundColor = Colors.White,
                    HasShadow = true,
                    BorderColor = Colors.Transparent,
                    Margin = new Thickness(0, 0, 0, 8),
                    Content = grid
                };

                DetailsContainer.Children.Add(frame);
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}