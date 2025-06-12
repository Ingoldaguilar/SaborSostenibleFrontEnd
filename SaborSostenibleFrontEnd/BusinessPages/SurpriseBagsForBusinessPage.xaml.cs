using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.BusinessPages
{
    public partial class SurpriseBagsForBusinessPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        const string _placeholderUrl = "http://34.39.128.125/Uploads/Logos/bolsaSorpresaEstandar.jpg";

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

            var resp = await _api.GetAsync<ResSurpriseBagsForBusiness>("surpriseBagsForBusiness/get");
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
                    VerticalOptions = LayoutOptions.CenterAndExpand
                });
                return;
            }

            foreach (var bag in resp.SurpriseBagForBusiness)
            {
                var frame = new Frame
                {
                    CornerRadius = 10,
                    Padding = 0,
                    BackgroundColor = Color.FromArgb("#FFF5F5F5"),
                    HasShadow = true
                };

                var layout = new VerticalStackLayout { Spacing = 0 };

                // Imagen de bolsa + descripción y fecha
                var header = new HorizontalStackLayout
                {
                    Padding = new Thickness(12, 10),
                    Spacing = 10,
                    VerticalOptions = LayoutOptions.Center
                };
                header.Children.Add(new Image
                {
                    Source = ImageSource.FromUri(new Uri(_placeholderUrl)),
                    WidthRequest = 40,
                    HeightRequest = 40,
                    Aspect = Aspect.AspectFill
                });
                header.Children.Add(new Label
                {
                    Text = bag.Description,
                    FontSize = 16,
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center
                });
                // Fecha a la derecha
                header.Children.Add(new Label
                {
                    Text = bag.CreatedAt.ToLocalTime().ToString("d/M/yyyy"),
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.EndAndExpand,
                    VerticalOptions = LayoutOptions.Center
                });
                layout.Children.Add(header);

                // Precio y estado
                var infoRow = new HorizontalStackLayout
                {
                    Padding = new Thickness(12, 0, 12, 0),
                    Spacing = 10,
                    VerticalOptions = LayoutOptions.Center
                };
                infoRow.Children.Add(new Label
                {
                    Text = $"{bag.Price} colones",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                infoRow.Children.Add(new Label
                {
                    Text = bag.State,
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                layout.Children.Add(infoRow);

                frame.Content = layout;
                BagsContainer.Children.Add(frame);
            }

            // Ver más al final
            var more = new Label
            {
                Text = "Ver más ?",
                FontSize = 14,
                TextColor = Color.FromArgb("#2E7D32"),
                HorizontalOptions = LayoutOptions.Center
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, __) => { /* lógica de cargar más */ };
            more.GestureRecognizers.Add(tap);
            BagsContainer.Children.Add(more);
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}
