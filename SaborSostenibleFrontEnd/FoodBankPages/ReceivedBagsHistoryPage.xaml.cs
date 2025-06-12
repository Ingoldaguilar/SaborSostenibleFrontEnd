using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
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
                // Frame con esquinas suaves y margen inferior
                var frame = new Frame
                {
                    CornerRadius = 6,
                    Margin = new Thickness(0, 0, 0, 4),
                    Padding = 0,
                    BackgroundColor = Colors.White,
                    HasShadow = true,
                    BorderColor = Color.FromRgba(0, 0, 0, 0.02)
                };

                // Grid: 3 columnas (logo, info, badge) × 3 filas
                var grid = new Grid
                {
                    Padding = new Thickness(6),
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = 56 },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto }
                    },
                    ColumnSpacing = 6,
                    RowSpacing = 1
                };

                // 1) Logo circular
                var logoImage = new Image
                {
                    Source = ImageSource.FromUri(new Uri(_baseUrl + item.LogoUrl)),
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
                Grid.SetRowSpan(logoImage, 3);
                Grid.SetColumn(logoImage, 0);
                grid.Children.Add(logoImage);

                // 2) Nombre del restaurante
                var nameLabel = new Label
                {
                    Text = item.BusinessName,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 11,
                    TextColor = Colors.Black,
                    LineBreakMode = LineBreakMode.WordWrap
                };
                Grid.SetRow(nameLabel, 0);
                Grid.SetColumn(nameLabel, 1);
                grid.Children.Add(nameLabel);

                // 3) Nombre de la bolsa (verde)
                var bagLabel = new Label
                {
                    Text = item.BagDescription,
                    FontSize = 10,
                    TextColor = Color.FromArgb("#2E7D32"),
                    LineBreakMode = LineBreakMode.WordWrap
                };
                Grid.SetRow(bagLabel, 1);
                Grid.SetColumn(bagLabel, 1);
                grid.Children.Add(bagLabel);

                // 4) Fecha con icono
                var dateLayout = new HorizontalStackLayout
                {
                    Spacing = 4,
                    VerticalOptions = LayoutOptions.Center
                };
                dateLayout.Children.Add(new Label
                {
                    Text = "\uf133", // icono calendario FontAwesome
                    FontFamily = "FontAwesome",
                    TextColor = Colors.Gray,
                    FontSize = 9,
                    VerticalOptions = LayoutOptions.Center,
                });
                dateLayout.Children.Add(new Label
                {
                    Text = item.DonationDate.ToLocalTime().ToString("dd/MM/yyyy"),
                    FontSize = 9,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                Grid.SetRow(dateLayout, 2);
                Grid.SetColumn(dateLayout, 1);
                grid.Children.Add(dateLayout);

                // 5) Badge “Completado”
                var badgeFrame = new Frame
                {
                    Padding = new Thickness(12, 6),
                    BackgroundColor = Color.FromArgb("#2E7D32"),
                    CornerRadius = 16,
                    HeightRequest = 26,
                    HasShadow = false,
                    VerticalOptions = LayoutOptions.Center
                };
                badgeFrame.Content = new HorizontalStackLayout
                {
                    Spacing = 4,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label
                        {
                            Text = "\uf058", // ícono check-circle FontAwesome
                            FontFamily = "FontAwesome",
                            FontSize = 9,
                            TextColor = Colors.White,
                            VerticalOptions = LayoutOptions.Center
                        },
                        new Label
                        {
                            Text = item.State,
                            FontSize = 9,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White,
                            VerticalOptions = LayoutOptions.Center
                        }
                    }
                };
                Grid.SetRowSpan(badgeFrame, 3);
                Grid.SetColumn(badgeFrame, 2);
                grid.Children.Add(badgeFrame);

                // Ensamblar y agregar
                frame.Content = grid;
                HistoryContainer.Children.Add(frame);
            }
        }
    }
}
