using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.AdminPages
{
    public partial class ListFoodBanksPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();

        public ListFoodBanksPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadFoodBanksAsync();
        }

        private async Task LoadFoodBanksAsync()
        {
            BanksContainer.Children.Clear();

            var resp = await _api.GetAsync<ResAllFoodBanksDetails>("allFoodBanksDetails/get");
            if (resp == null)
            {
                await DisplayAlert("Error", "No se pudo conectar al servidor.", "OK");
                return;
            }

            if (!resp.Success || resp.FoodBanks?.Any() != true)
            {
                BanksContainer.Children.Add(new Label
                {
                    Text = "Actualmente no hay bancos de alimentos.",
                    FontSize = 16,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                });
                return;
            }

            const string baseUrl = "http://34.39.128.125/";

            foreach (var bank in resp.FoodBanks)
            {
                // 1) Frame tarjeta
                var frame = new Frame
                {
                    CornerRadius = 6,
                    Padding = 0,
                    Margin = new Thickness(0, 0, 0, 10),
                    BackgroundColor = Colors.White,
                    HasShadow = true,
                    BorderColor = Color.FromRgba(0, 0, 0, 0.02),
                    HorizontalOptions = LayoutOptions.Fill
                };

                // 2) Grid de 3 columnas
                var grid = new Grid
                {
                    Padding = new Thickness(12),
                    ColumnSpacing = 20,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Fill,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(40) },    // logo 40px
                        new ColumnDefinition { Width = GridLength.Star },        // info
                        new ColumnDefinition { Width = GridLength.Auto }         // switch
                    }
                };

                // 3) Logo circular
                var logo = new Image
                {
                    Source = ImageSource.FromUri(new Uri($"{baseUrl}{bank.LogoImage}")),
                    WidthRequest = 40,
                    HeightRequest = 40,
                    Aspect = Aspect.AspectFill,
                    Clip = new EllipseGeometry
                    {
                        Center = new Point(20, 20),
                        RadiusX = 20,
                        RadiusY = 20
                    },
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start
                };
                grid.Add(logo, 0, 0);

                // 4) Info: nombre (warp) + teléfono
                var info = new VerticalStackLayout
                {
                    Spacing = 4,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                // Nombre con wrap
                info.Children.Add(new Label
                {
                    Text = bank.Name,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 13,
                    TextColor = Colors.Black,
                    LineBreakMode = LineBreakMode.WordWrap
                });
                // Teléfono con icono
                var phoneLayout = new HorizontalStackLayout
                {
                    Spacing = 4,
                    VerticalOptions = LayoutOptions.Center
                };
                phoneLayout.Children.Add(new Label
                {
                    Text = "\uf095", // FontAwesome phone
                    FontFamily = "FontAwesome",
                    FontSize = 10,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                phoneLayout.Children.Add(new Label
                {
                    Text = bank.PhoneNumber,
                    FontSize = 12,
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center
                });
                info.Children.Add(phoneLayout);

                grid.Add(info, 1, 0);

                // 5) Switch
                var toggle = new Switch
                {
                    IsToggled = bank.IsActive,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center
                };
                toggle.Toggled += async (_, args) =>
                    await UpdateIsActiveAsync(bank.FoodBankId, args.Value, toggle);

                grid.Add(toggle, 2, 0);

                // 6) Montar y agregar
                frame.Content = grid;
                BanksContainer.Children.Add(frame);
            }
        }

        private async Task UpdateIsActiveAsync(int id, bool isActive, Switch toggle)
        {
            // loader opcional
            var loader = new LoadingPage();
            await Navigation.PushModalAsync(loader);

            var req = new ReqFoodBankIsActiveUpdate
            {
                FoodBankId = id,
                IsActive = isActive
            };

            var res = await _api.PostAsync<ReqFoodBankIsActiveUpdate, ResBase>(
                "foodBankIsActive/update", req);

            await Navigation.PopModalAsync();

            if (res?.Success == true)
            {
                await DisplayAlert(
                    "Éxito",
                    isActive ? "Banco activado" : "Banco desactivado",
                    "OK");
            }
            else
            {
                // revertir estado si falló
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
