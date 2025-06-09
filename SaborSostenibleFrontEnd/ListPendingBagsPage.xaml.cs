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
                var frame = new Frame
                {
                    CornerRadius = 10,
                    Padding = 0,
                    BackgroundColor = Colors.White,
                    HasShadow = true
                };

                var layout = new VerticalStackLayout { Spacing = 0 };

                // Encabezado: logo + negocio
                var header = new HorizontalStackLayout
                {
                    Spacing = 10,
                    Padding = new Thickness(12, 10),
                    VerticalOptions = LayoutOptions.Center
                };
                header.Children.Add(new Image
                {
                    Source = ImageSource.FromUri(new Uri(_baseUrl + bag.LogoUrl)),
                    WidthRequest = 40,
                    HeightRequest = 40,
                    Aspect = Aspect.AspectFill
                });
                header.Children.Add(new Label
                {
                    Text = bag.BusinessName,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16,
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center
                });
                layout.Children.Add(header);

                // Fecha + descripción
                var descRow = new HorizontalStackLayout
                {
                    Spacing = 6,
                    Padding = new Thickness(12, 0),
                    VerticalOptions = LayoutOptions.Center
                };
                descRow.Children.Add(new Image
                {
                    Source = "calendar.png",
                    WidthRequest = 16,
                    HeightRequest = 16,
                    Aspect = Aspect.AspectFit
                });
                descRow.Children.Add(new Label
                {
                    Text = bag.DonationDate.ToLocalTime().ToString("dd/MM/yyyy"),
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                descRow.Children.Add(new Label
                {
                    Text = bag.BagDescription,
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                layout.Children.Add(descRow);

                // Botón Pendiente que navega
                var btn = new Button
                {
                    Visual = VisualMarker.Default,
                    Text = bag.State,
                    BackgroundColor = Color.FromArgb("#556B2F"),
                    TextColor = Colors.White,
                    CornerRadius = 0,
                    FontSize = 14,
                    HeightRequest = 32,
                    Padding = new Thickness(20, 0)
                };
                btn.Clicked += (_, __) =>
                    Navigation.PushAsync(new ConfirmReceptionPage(bag));
                layout.Children.Add(btn);

                frame.Content = layout;
                BagsContainer.Children.Add(frame);
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e) =>
            _ = Navigation.PopAsync();
    }
}
