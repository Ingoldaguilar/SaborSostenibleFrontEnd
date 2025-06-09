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
    public partial class PendingSalesHistoryPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private const string _baseUrl = "http://34.39.128.125/";

        public PendingSalesHistoryPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadPendingSalesAsync();
        }

        private async Task LoadPendingSalesAsync()
        {
            SalesContainer.Children.Clear();

            var resp = await _api.GetAsync<ResPendingSalesHistory>("businessPendingSalesHistory/get");
            if (resp == null)
            {
                await DisplayAlert("Error", "No se pudo conectar al servidor.", "OK");
                return;
            }

            if (!resp.Success || resp.PendingSalesHistory?.Any() != true)
            {
                SalesContainer.Children.Add(new Label
                {
                    Text = "Actualmente no hay ventas en proceso.",
                    FontSize = 16,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                });
                return;
            }

            foreach (var order in resp.PendingSalesHistory)
            {
                var frame = new Frame
                {
                    CornerRadius = 10,
                    Padding = 0,
                    BackgroundColor = Colors.White,
                    HasShadow = true
                };

                var layout = new VerticalStackLayout { Spacing = 0 };

                // Header: código + logo
                var header = new HorizontalStackLayout
                {
                    Padding = new Thickness(12, 10),
                    Spacing = 8,
                    VerticalOptions = LayoutOptions.Center
                };
                header.Children.Add(new Label
                {
                    Text = order.OrderCode,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 14,
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center
                });
                header.Children.Add(new Image
                {
                    Source = ImageSource.FromUri(new Uri(_baseUrl + order.LogoImage)),
                    WidthRequest = 24,
                    HeightRequest = 24,
                    Aspect = Aspect.AspectFill
                });
                layout.Children.Add(header);

                // Fecha, hora y monto
                var infoRow = new HorizontalStackLayout
                {
                    Padding = new Thickness(12, 0),
                    Spacing = 20,
                    VerticalOptions = LayoutOptions.Center
                };
                // Fecha
                infoRow.Children.Add(new HorizontalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Image
                        {
                            Source        = "calendar.png",
                            WidthRequest  = 16,
                            HeightRequest = 16,
                            Aspect        = Aspect.AspectFit
                        },
                        new Label
                        {
                            Text           = order.OrderDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                            FontSize       = 14,
                            TextColor      = Colors.Gray,
                            VerticalOptions= LayoutOptions.Center
                        }
                    }
                });
                // Total
                infoRow.Children.Add(new HorizontalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label
                        {
                            Text           = order.TotalAmount.ToString() + " colones",
                            FontSize       = 14,
                            TextColor      = Colors.Gray,
                            VerticalOptions = LayoutOptions.Center
                        }
                    }
                });

                layout.Children.Add(infoRow);

                // Botón “Pendiente”
                var btn = new Button
                {
                    Visual = VisualMarker.Default,
                    Text = order.StateText,
                    BackgroundColor = Color.FromArgb("#3E7B31"),
                    TextColor = Colors.White,
                    CornerRadius = 0,
                    FontSize = 14,
                    HeightRequest = 32,
                    Padding = new Thickness(20, 0)
                };
                btn.Clicked += (_, __) =>
                    Navigation.PushAsync(new BusinessPendingSaleDetailsPage(order.OrderId));

                layout.Children.Add(btn);

                frame.Content = layout;
                SalesContainer.Children.Add(frame);
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}
