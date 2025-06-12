using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd
{
    public partial class InsertSurpriseBagPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private int _selectedFoodBankId;
        private bool _isDonation;

        public InsertSurpriseBagPage()
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
            var resp = await _api.GetAsync<ResFoodBanksIdAndName>("foodBanksIdAndName/get");
            if (resp?.Success == true && resp.FoodBanks != null)
            {
                FoodBankPicker.ItemsSource = resp.FoodBanks;
                FoodBankPicker.ItemDisplayBinding = new Binding("Name");
                FoodBankPicker.SelectedIndexChanged += (_, __) =>
                {
                    if (FoodBankPicker.SelectedIndex >= 0)
                        _selectedFoodBankId = resp.FoodBanks[FoodBankPicker.SelectedIndex].FoodBankId;
                };
            }
            else
            {
                await DisplayAlert("Error", "No se cargaron bancos de alimentos.", "OK");
            }
        }

        private void OnDonationToggled(object sender, ToggledEventArgs e)
        {
            _isDonation = e.Value;

            // Cuando es donación, se muestra Picker, se oculta Precio
            PriceContainer.IsVisible = !_isDonation;
            FoodBankContainer.IsVisible = _isDonation;
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
            {
                DisplayAlert("Error", "Ingrese descripción.", "OK");
                return false;
            }

            if (_isDonation)
            {
                if (_selectedFoodBankId == 0)
                {
                    DisplayAlert("Error", "Seleccione un banco de alimentos.", "OK");
                    return false;
                }
            }
            else
            {
                if (!decimal.TryParse(PriceEntry.Text, out var p) || p <= 0)
                {
                    DisplayAlert("Error", "Ingrese un precio válido.", "OK");
                    return false;
                }
            }

            return true;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            if (!Validate()) return;

            decimal priceValue = 1m;
            if (!_isDonation)
                priceValue = decimal.Parse(PriceEntry.Text, CultureInfo.InvariantCulture);

            var req = new ReqInsertSurpriseBag
            {
                IsDonation = _isDonation,
                Description = DescriptionEntry.Text.Trim(),
                Price = priceValue,
                FoodBankId = _isDonation ? _selectedFoodBankId : 0
            };

            var loader = new LoadingPage();
            await Navigation.PushModalAsync(loader);

            var res = await _api.PostAsync<ReqInsertSurpriseBag, ResBase>(
                "surpriseBag/insert", req);

            await Navigation.PopModalAsync();

            if (res?.Success == true)
            {
                // Limpia todos los campos
                DescriptionEntry.Text = string.Empty;
                PriceEntry.Text = string.Empty;
                DonationSwitch.IsToggled = false;
                FoodBankPicker.SelectedIndex = -1;

                await DisplayAlert("Éxito", "Bolsa registrada correctamente.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                var errs = res?.Errors?.Select(x => x.Description)
                           ?? new[] { "Error desconocido" };
                await DisplayAlert("Error", string.Join("\n", errs), "OK");
            }
        }
    }
}
