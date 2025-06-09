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
    public partial class ListBusinessesPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();

        public ListBusinessesPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadBusinessesAsync();
        }

        private async Task LoadBusinessesAsync()
        {
            BusinessesContainer.Children.Clear();

            var resp = await _api.GetAsync<ResAllBusinessDetails>("allBusinessDetails/get");
            if (resp == null)
            {
                await DisplayAlert("Error", "No se pudo conectar al servidor.", "OK");
                return;
            }

            if (!resp.Success || resp.Businesses?.Any() != true)
            {
                BusinessesContainer.Children.Add(new Label
                {
                    Text = "Actualmente no hay restaurantes.",
                    FontSize = 16,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                });
                return;
            }

            const string imageBaseUrl = "http://34.39.128.125/";

            foreach (var biz in resp.Businesses)
            {
                var frame = new Frame
                {
                    CornerRadius = 10,
                    Padding = 12,
                    BackgroundColor = Colors.White,
                    HasShadow = true
                };

                // Grid 3 columnas: logo | info | toggle
                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition{ Width = GridLength.Auto },
                        new ColumnDefinition{ Width = GridLength.Star },
                        new ColumnDefinition{ Width = GridLength.Auto }
                    },
                    VerticalOptions = LayoutOptions.Center
                };

                // 1) Logo
                var img = new Image
                {
                    Source = ImageSource.FromUri(new Uri($"{imageBaseUrl}{biz.LogoImage}")),
                    WidthRequest = 40,
                    HeightRequest = 40,
                    Aspect = Aspect.AspectFill
                };
                grid.Add(img, 0, 0);

                // 2) Nombre + teléfono
                var info = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(10, 0) };
                info.Children.Add(new Label
                {
                    Text = biz.Name,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16,
                    TextColor = Colors.Black
                });
                info.Children.Add(new Label
                {
                    Text = biz.PhoneNumber,
                    FontSize = 14,
                    TextColor = Colors.Gray
                });
                grid.Add(info, 1, 0);

                // 3) Switch de activo/inactivo
                var toggle = new Switch
                {
                    IsToggled = biz.IsActive,
                    HorizontalOptions = LayoutOptions.End
                };
                toggle.Toggled += async (_, args) =>
                    await UpdateIsActiveAsync(biz.BusinessId, args.Value, toggle);
                grid.Add(toggle, 2, 0);

                frame.Content = grid;
                BusinessesContainer.Children.Add(frame);
            }
        }

        private async Task UpdateIsActiveAsync(int id, bool isActive, Switch toggle)
        {
            // loader opcional
            var loader = new LoadingPage();
            await Navigation.PushModalAsync(loader);

            var req = new ReqBusinessIsActiveUpdate
            {
                BusinessId = id,
                IsActive = isActive
            };

            var res = await _api.PostAsync<ReqBusinessIsActiveUpdate, ResBase>(
                "businessIsActive/update", req);

            await Navigation.PopModalAsync();

            if (res?.Success == true)
            {
                await DisplayAlert(
                    "Éxito",
                    isActive ? "Restaurante activado" : "Restaurante desactivado",
                    "OK");
            }
            else
            {
                // revertir toggle si falla
                toggle.IsToggled = !isActive;
                var errs = res?.Errors?.Select(x => x.Description)
                            ?? new[] { "Error desconocido" };
                await DisplayAlert("Error", string.Join("\n", errs), "OK");
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}
