using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.FoodBankPages
{
    public partial class ListPendingBagsPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private const string _baseUrl = "http://34.39.128.125/";

        public ListPendingBagsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadPendingBagsAsync();
        }

        private async Task LoadPendingBagsAsync()
        {
            BagsContainer.Children.Clear();

            var resp = await _api.GetAsync<ResPendingBagsByFoodBank>("pendingBagsByFoodBank/get");
            if (resp == null)
            {
                await DisplayAlert("Error", "No se pudo conectar al servidor.", "OK");
                return;
            }

            if (!resp.Success || resp.PendingBags?.Any() != true)
            {
                BagsContainer.Children.Add(new Label
                {
                    Text = "Actualmente no hay bolsas pendientes.",
                    FontSize = 16,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                });
                return;
            }

            foreach (var bag in resp.PendingBags)
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

                // Grid: 3 columnas (logo, info, botón) × 3 filas
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
                    Source = ImageSource.FromUri(new Uri(_baseUrl + bag.LogoUrl)),
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
                    Text = bag.BusinessName,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 11,
                    TextColor = Colors.Black,
                    LineBreakMode = LineBreakMode.WordWrap
                };
                Grid.SetRow(nameLabel, 0);
                Grid.SetColumn(nameLabel, 1);
                grid.Children.Add(nameLabel);

                // 3) Nombre de la bolsa
                var bagLabel = new Label
                {
                    Text = bag.BagDescription,
                    FontSize = 10,
                    TextColor = Color.FromArgb("#789262"),
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
                    Text = "\uf133", // Emoji de calendario
                    FontFamily = "FontAwesome",
                    TextColor = Colors.Gray,
                    FontSize = 9,
                    VerticalOptions = LayoutOptions.Center,
                });

                // Fecha
                dateLayout.Children.Add(new Label
                {
                    Text = bag.DonationDate.ToLocalTime().ToString("dd/MM/yyyy"),
                    FontSize = 9,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });

                Grid.SetRow(dateLayout, 2);
                Grid.SetColumn(dateLayout, 1);
                grid.Children.Add(dateLayout);

                // 5) Botón “Pendiente”
                var btn = new Button
                {
                    Text = bag.State,
                    BackgroundColor = Color.FromArgb("#789262"),
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    CornerRadius = 16,
                    FontSize = 9,
                    HeightRequest = 26,
                    Padding = new Thickness(12, 6),
                    Visual = VisualMarker.Default,
                    VerticalOptions = LayoutOptions.Center
                };
                btn.Clicked += (_, __) => Navigation.PushAsync(new ConfirmReceptionPage(bag));
                Grid.SetRowSpan(btn, 3);
                Grid.SetColumn(btn, 2);
                grid.Children.Add(btn);

                // Ensamblar y agregar
                frame.Content = grid;
                BagsContainer.Children.Add(frame);
            }
        }
    }
}
