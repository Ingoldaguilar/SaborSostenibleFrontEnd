using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.BusinessPages
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
                // Card frame
                var frame = new Frame
                {
                    CornerRadius = 6,
                    Margin = new Thickness(0, 0, 0, 4),
                    Padding = 0,
                    BackgroundColor = Colors.White,
                    HasShadow = true,
                    BorderColor = Color.FromRgba(0, 0, 0, 0.02)
                };

                // Contenido horizontal
                var content = new HorizontalStackLayout
                {
                    Padding = new Thickness(12),
                    Spacing = 20,
                    VerticalOptions = LayoutOptions.Center
                };

                // Logo circular
                var logo = new Image
                {
                    Source = ImageSource.FromUri(new Uri(_baseUrl + order.LogoImage)),
                    WidthRequest = 56,
                    HeightRequest = 56,
                    Aspect = Aspect.AspectFill,
                    Clip = new EllipseGeometry
                    {
                        Center = new Point(28, 28),
                        RadiusX = 28,
                        RadiusY = 28
                    }
                };
                content.Children.Add(logo);

                // Detalles (código, monto, fecha)
                var details = new VerticalStackLayout
                {
                    Spacing = 4,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };

                details.Children.Add(new Label
                {
                    Text = order.OrderCode,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 11,
                    TextColor = Colors.Black,
                    LineBreakMode = LineBreakMode.TailTruncation
                });

                details.Children.Add(new Label
                {
                    Text = $"\u20A1{order.TotalAmount:N0}",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 10,
                    TextColor = Color.FromArgb("#789262")
                });

                var dateLayout = new HorizontalStackLayout
                {
                    Spacing = 4,
                    VerticalOptions = LayoutOptions.Center
                };
                // Icono reloj (FontAwesome \uf017)
                dateLayout.Children.Add(new Label
                {
                    Text = "\uf017",
                    FontFamily = "FontAwesome",
                    FontSize = 9,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                dateLayout.Children.Add(new Label
                {
                    Text = order.OrderDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    FontSize = 9,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                details.Children.Add(dateLayout);

                content.Children.Add(details);

                // Botón “Pendiente”
                var btn = new Button
                {
                    Text = order.StateText,
                    BackgroundColor = Color.FromArgb("#789262"),
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    CornerRadius = 16,
                    FontSize = 10,
                    HeightRequest = 26,
                    Padding = new Thickness(12, 6),
                    Visual = VisualMarker.Default,
                    VerticalOptions = LayoutOptions.Center
                };
                btn.Clicked += (_, __) =>
                    Navigation.PushAsync(new BusinessPendingSaleDetailsPage(order.OrderId));

                content.Children.Add(btn);

                frame.Content = content;
                SalesContainer.Children.Add(frame);
            }
        }
    }
}
