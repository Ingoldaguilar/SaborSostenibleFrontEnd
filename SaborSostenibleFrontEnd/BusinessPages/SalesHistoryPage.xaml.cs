using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.BusinessPages
{
    public partial class SalesHistoryPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private const string baseUrl = "http://34.39.128.125/";

        public SalesHistoryPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadSalesHistoryAsync();
        }

        private async Task LoadSalesHistoryAsync()
        {
            HistoryContainer.Children.Clear();

            var resp = await _api.GetAsync<ResSalesHistory>("businessSalesHistory/get");
            if (resp == null)
            {
                await DisplayAlert("Error", "No se pudo conectar al servidor.", "OK");
                return;
            }

            if (!resp.Success || resp.SalesHistory?.Any() != true)
            {
                HistoryContainer.Children.Add(new Label
                {
                    Text = "Actualmente no hay historial de ventas.",
                    FontSize = 16,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                });
                return;
            }

            foreach (var order in resp.SalesHistory)
            {
                var frame = new Frame
                {
                    CornerRadius = 10,
                    Padding = 0,
                    BackgroundColor = Color.FromArgb("#FFF5F5F5"),
                    HasShadow = true
                };

                var layout = new VerticalStackLayout { Spacing = 0 };

                // 1) Código de orden + logo
                var header = new HorizontalStackLayout
                {
                    Padding = new Thickness(12, 10),
                    Spacing = 10,
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
                    Source = ImageSource.FromUri(new Uri(baseUrl + order.LogoImage)),
                    WidthRequest = 24,
                    HeightRequest = 24,
                    Aspect = Aspect.AspectFill
                });

                layout.Children.Add(header);

                // 2) Fecha + hora + total
                var infoRow = new HorizontalStackLayout
                {
                    Padding = new Thickness(12, 0),
                    Spacing = 20,
                    VerticalOptions = LayoutOptions.Center
                };

                infoRow.Children.Add(new HorizontalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Image
                        {
                            Source = "calendar.png",
                            WidthRequest = 16,
                            HeightRequest= 16,
                            Aspect = Aspect.AspectFit
                        },
                        new Label
                        {
                            Text = order.OrderDate
                                .ToLocalTime()
                                .ToString("dd/MM/yyyy HH:mm"),
                            FontSize = 14,
                            TextColor= Colors.Gray,
                            VerticalOptions = LayoutOptions.Center
                        }
                    }
                });

                infoRow.Children.Add(new HorizontalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Label
                        {
                            Text      = order.TotalAmount.ToString() + " colones",
                            FontSize  = 14,
                            TextColor = Colors.Gray,
                            VerticalOptions = LayoutOptions.Center
                        }
                    }
                });

                layout.Children.Add(infoRow);

                // 3) Botón estado
                var btn = new Button
                {
                    Visual = VisualMarker.Default,
                    Text = order.StateText,
                    BackgroundColor = Color.FromArgb("#2E7D32"),
                    TextColor = Colors.White,
                    CornerRadius = 0,
                    FontSize = 14,
                    HeightRequest = 32,
                    Padding = new Thickness(20, 0)
                };
                btn.Clicked += (_, __) =>
                    Navigation.PushAsync(new OrderDetailsPage(order.OrderId));

                layout.Children.Add(btn);

                frame.Content = layout;
                HistoryContainer.Children.Add(frame);
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}
