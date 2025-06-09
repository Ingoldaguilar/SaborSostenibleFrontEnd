using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd
{
    public partial class UpdateFoodBankPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private int _foodBankId;
        private string _logoImageBase64;
        private FileResult _logoFileResult;
        private decimal _latitude;
        private decimal _longitude;

        public UpdateFoodBankPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadFoodBankAsync();
        }

        private async Task LoadFoodBankAsync()
        {
            try
            {
                var resp = await _api.GetAsync<ResFoodBankForUpdate>("foodBankForUpdate/get");
                if (resp == null || !resp.Success || resp.FoodBank == null)
                {
                    var msg = resp == null
                        ? "Sin respuesta del servidor."
                        : string.Join("\n", resp.Errors.Select(e => e.Description));
                    await DisplayAlert("Error", msg, "OK");
                    return;
                }

                var fb = resp.FoodBank;
                _foodBankId = fb.FoodBankId;
                NameEntry.Text = fb.Name;
                DescriptionEntry.Text = fb.Description;
                _logoImageBase64 = fb.LogoImage;            // base64 existing or URL
                PhoneEntry.Text = fb.PhoneNumber;
                AddressEntry.Text = fb.Address;
                LatitudeEntry.Text = fb.Latitude.ToString(CultureInfo.InvariantCulture);
                LongitudeEntry.Text = fb.Longitude.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Cargando datos: {ex.Message}", "OK");
            }
        }

        //private async void OnSelectLogoClicked(object sender, EventArgs e)
        //{
        //    var result = await FilePicker.PickAsync(new PickOptions
        //    {
        //        FileTypes = FilePickerFileType.Images
        //    });
        //    if (result != null)
        //    {
        //        _logoFileResult = result;
        //        using var stream = await result.OpenReadAsync();
        //        using var ms = new MemoryStream();
        //        await stream.CopyToAsync(ms);
        //        _logoImageBase64 = Convert.ToBase64String(ms.ToArray());
        //    }
        //}

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text)
             || string.IsNullOrWhiteSpace(DescriptionEntry.Text)
             || string.IsNullOrWhiteSpace(_logoImageBase64)
             || string.IsNullOrWhiteSpace(PhoneEntry.Text)
             || string.IsNullOrWhiteSpace(AddressEntry.Text)
             || string.IsNullOrWhiteSpace(LatitudeEntry.Text)
             || string.IsNullOrWhiteSpace(LongitudeEntry.Text))
            {
                DisplayAlert("Error", "Complete todos los campos.", "OK");
                return false;
            }

            if (!decimal.TryParse(LatitudeEntry.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out _latitude)
             || !decimal.TryParse(LongitudeEntry.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out _longitude))
            {
                DisplayAlert("Error", "Latitud o longitud inválidas.", "OK");
                return false;
            }

            return true;
        }

        private async void OnUpdateClicked(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            var loader = new LoadingPage();
            await Navigation.PushModalAsync(loader);

            var req = new ReqUpdateFoodBank
            {
                FoodBankId = _foodBankId,
                Name = NameEntry.Text.Trim(),
                Description = DescriptionEntry.Text.Trim(),
                LogoImage = _logoImageBase64,
                PhoneNumber = PhoneEntry.Text.Trim(),
                Address = AddressEntry.Text.Trim(),
                Latitude = _latitude,
                Longitude = _longitude
            };

            try
            {
                var res = await _api.PostAsync<ReqUpdateFoodBank, ResBase>("foodBank/update", req);

                if (res?.Success == true)
                {
                    await DisplayAlert("Éxito", "FoodBank actualizado.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    var errs = res?.Errors?.Select(x => x.Description) ?? new[] { "Error desconocido" };
                    await DisplayAlert("Error", string.Join("\n", errs), "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Actualizando: {ex.Message}", "OK");
            }
            finally
            {
                await Navigation.PopModalAsync();
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}
