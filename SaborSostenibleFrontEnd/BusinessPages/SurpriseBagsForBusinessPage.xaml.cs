using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.BusinessPages
{
    public partial class SurpriseBagsForBusinessPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        const string _placeholderUrl =
            "http://34.39.128.125/Uploads/Logos/bolsaSorpresaEstandar.jpg";

        public SurpriseBagsForBusinessPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadBagsAsync();
        }

        private async Task LoadBagsAsync()
        {
            BagsContainer.Children.Clear();

            var resp = await _api.GetAsync<ResSurpriseBagsForBusiness>(
                "surpriseBagsForBusiness/get");
            if (resp == null)
            {
                await DisplayAlert("Error", "No se pudo conectar al servidor.", "OK");
                return;
            }

            if (!resp.Success || resp.SurpriseBagForBusiness?.Any() != true)
            {
                BagsContainer.Children.Add(new Label
                {
                    Text = "Actualmente no hay bolsas sorpresa.",
                    FontSize = 16,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                });
                return;
            }

            foreach (var bag in resp.SurpriseBagForBusiness)
            {
                // tarjeta
                var frame = new Frame
                {
                    CornerRadius = 6,
                    Margin = new Thickness(0, 0, 0, 4),
                    Padding = 0,
                    BackgroundColor = Colors.White,
                    HasShadow = true,
                    BorderColor = Color.FromRgba(0, 0, 0, 0.02)
                };

                // Grid con tres columnas y separación uniforme
                var contentGrid = new Grid
                {
                    Padding = new Thickness(12),
                    ColumnSpacing = 20,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(56) },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    }
                };

                // 1) Imagen circular
                var logo = new Image
                {
                    Source = ImageSource.FromUri(new Uri(_placeholderUrl)),
                    WidthRequest = 56,
                    HeightRequest = 56,
                    Aspect = Aspect.AspectFill,
                    Clip = new EllipseGeometry
                    {
                        Center = new Point(28, 28),
                        RadiusX = 28,
                        RadiusY = 28
                    },
                    VerticalOptions = LayoutOptions.Start
                };
                Grid.SetColumn(logo, 0);
                contentGrid.Children.Add(logo);

                // 2) Detalles (descripción, precio, fecha)
                var details = new VerticalStackLayout
                {
                    Spacing = 4,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };

                // Descripción con wrap
                details.Children.Add(new Label
                {
                    Text = bag.Description,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 12,
                    TextColor = Colors.Black,
                    LineBreakMode = LineBreakMode.WordWrap
                });

                // Precio en verde con símbolo ?
                details.Children.Add(new Label
                {
                    Text = $"\u20A1{bag.Price:N0}",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 11,
                    TextColor = Color.FromArgb("#789262")
                });

                // Fecha con icono
                var dateLayout = new HorizontalStackLayout
                {
                    Spacing = 4,
                    VerticalOptions = LayoutOptions.Center
                };
                dateLayout.Children.Add(new Label
                {
                    Text = "\uf133",
                    FontFamily = "FontAwesome",
                    FontSize = 9,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                dateLayout.Children.Add(new Label
                {
                    Text =
                        bag.CreatedAt
                           .ToLocalTime()
                           .ToString("dd/MM/yyyy"),
                    FontSize = 9,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                details.Children.Add(dateLayout);

                Grid.SetColumn(details, 1);
                contentGrid.Children.Add(details);

                // 3) Badge en lugar de botón de estado
                var badgeFrame = new Frame
                {
                    Padding = new Thickness(12, 6),
                    BackgroundColor = Color.FromArgb("#789262"),
                    CornerRadius = 16,
                    HeightRequest = 26,
                    HasShadow = false,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.End
                };
                badgeFrame.Content = new HorizontalStackLayout
                {
                    Spacing = 4,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
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
                            Text = bag.State,
                            FontSize = 9,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White,
                            VerticalOptions = LayoutOptions.Center
                        }
                    }
                };
                Grid.SetColumn(badgeFrame, 2);
                contentGrid.Children.Add(badgeFrame);

                frame.Content = contentGrid;
                BagsContainer.Children.Add(frame);
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}
