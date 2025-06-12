using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
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

            foreach (var bank in resp.FoodBanks)
            {
                var frame = new Frame
                {
                    CornerRadius = 10,
                    Padding = 12,
                    BackgroundColor = Colors.White,
                    HasShadow = true,
                };

                // grid de 3 columnas: logo | info | switch
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

                const string baseUrl = "http://34.39.128.125/";

                // 1) Logo
                var img = new Image
                {
                    Source = ImageSource.FromUri(new Uri($"{baseUrl}{bank.LogoImage}")),
                    WidthRequest = 40,
                    HeightRequest = 40,
                    Aspect = Aspect.AspectFill
                };
                grid.Add(img, 0, 0);

                // 2) Nombre + teléfono
                var info = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(10, 0) };
                info.Children.Add(new Label
                {
                    Text = bank.Name,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16,
                    TextColor = Colors.Black
                });
                info.Children.Add(new Label
                {
                    Text = bank.PhoneNumber,
                    FontSize = 14,
                    TextColor = Colors.Gray
                });
                grid.Add(info, 1, 0);

                // 3) Switch
                var toggle = new Switch
                {
                    IsToggled = bank.IsActive,
                    HorizontalOptions = LayoutOptions.End
                };
                toggle.Toggled += async (_, args) =>
                    await UpdateIsActiveAsync(bank.FoodBankId, args.Value, toggle);

                grid.Add(toggle, 2, 0);

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
