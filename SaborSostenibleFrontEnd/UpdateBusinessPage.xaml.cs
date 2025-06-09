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
    public partial class UpdateBusinessPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private int _businessId;
        private string _logoImageBase64;
        private FileResult _logoFileResult;
        private decimal _latitude;
        private decimal _longitude;

        public UpdateBusinessPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadBusinessAsync();
        }

        private async Task LoadBusinessAsync()
        {
            try
            {
                var resp = await _api.GetAsync<ResBusinessForUpdate>("businessesForUpdate/get");
                if (resp == null || !resp.Success || resp.Business == null)
                {
                    var msg = resp == null
                        ? "Sin respuesta del servidor."
                        : string.Join("\n", resp.Errors.Select(e => e.Description));
                    await DisplayAlert("Error", msg, "OK");
                    return;
                }

                var b = resp.Business;
                _businessId = b.BusinessId;
                NameEntry.Text = b.Name;
                DescriptionEntry.Text = b.Description;
                _logoImageBase64 = b.LogoImage;
                PhoneEntry.Text = b.PhoneNumber;
                AddressEntry.Text = b.Address;
                LatitudeEntry.Text = b.Latitude.ToString(CultureInfo.InvariantCulture);
                LongitudeEntry.Text = b.Longitude.ToString(CultureInfo.InvariantCulture);
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

            var req = new ReqUpdateBusiness
            {
                BusinessId = _businessId,
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
                var res = await _api.PostAsync<ReqUpdateBusiness, ResBase>(
                    "business/update", req);

                if (res?.Success == true)
                {
                    await DisplayAlert("Éxito", "Restaurante actualizado.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    var errs = res?.Errors?.Select(x => x.Description)
                               ?? new[] { "Error desconocido" };
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
