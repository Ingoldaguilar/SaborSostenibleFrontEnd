using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SaborSostenibleFrontEnd.Entities;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.AdminPages
{
    public partial class InsertFoodBankPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private List<UserByCustomerRole> _users;
        private int _selectedAdminUserId;
        private FileResult _logoFileResult;
        private decimal _latitude;
        private decimal _longitude;

        public InsertFoodBankPage()
        {
            InitializeComponent();
            LoadAdminUsers();
        }

        private async void LoadAdminUsers()
        {
            try
            {
                var resp = await _api.GetAsync<ResUsersByCustomerRole>("allUsersByCustomerRole/get");
                if (resp != null && resp.Success)
                {
                    _users = resp.Users;
                    AdminPicker.ItemsSource = _users;
                }
                else
                {
                    var errs = resp?.Errors?.Select(e => e.Description) ?? new[] { "Error desconocido" };
                    await DisplayAlert("Error", "No se pudieron cargar usuarios: " + string.Join(", ", errs), "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al obtener usuarios: {ex.Message}", "OK");
            }
        }

        private void AdminPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AdminPicker.SelectedIndex >= 0)
                _selectedAdminUserId = _users[AdminPicker.SelectedIndex].UserId;
        }

        private async void OnSelectLogoClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync(new PickOptions { FileTypes = FilePickerFileType.Images });
            if (result != null)
            {
                _logoFileResult = result;
                LogoSelectedLabel.Text = result.FileName;
            }
        }

        private bool EsEmailValido(string email)
        {
            try { return new MailAddress(email).Address == email; }
            catch { return false; }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text)
             || string.IsNullOrWhiteSpace(DescriptionEntry.Text)
             || _selectedAdminUserId == 0
             || _logoFileResult == null
             || string.IsNullOrWhiteSpace(EmailEntry.Text)
             || string.IsNullOrWhiteSpace(PhoneEntry.Text)
             || string.IsNullOrWhiteSpace(AddressEntry.Text))
            {
                DisplayAlert("Error", "Complete todos los campos y seleccione un logo", "OK");
                return false;
            }

            if (!EsEmailValido(EmailEntry.Text))
            {
                DisplayAlert("Error", "Correo no v�lido", "OK");
                return false;
            }

            if (_latitude == 0 || _longitude == 0)
            {
                DisplayAlert("Error", "Debe seleccionar una ubicaci�n en el mapa", "OK");
                return false;
            }

            return true;
        }

        private async void OnElegirUbicacionClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ElegirUbicacionPage((ubicacion) =>
            {
                _latitude = (decimal)ubicacion.Latitude;
                _longitude = (decimal)ubicacion.Longitude;

                // Actualizar la UI desde el hilo principal
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UbicacionLabel.Text = $"Ubicaci�n elegida: {_latitude:F5}, {_longitude:F5}";
                });
            }));
        }

        private async void OnInsertClicked(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;

            var loader = new LoadingPage();
            await Navigation.PushModalAsync(loader);

            bool isSuccess = false;

            try
            {
                var req = new ReqInsertFoodBank
                {
                    Name = NameEntry.Text.Trim(),
                    Description = DescriptionEntry.Text.Trim(),
                    AdminUserId = _selectedAdminUserId,
                    Email = EmailEntry.Text.Trim(),
                    PhoneNumber = PhoneEntry.Text.Trim(),
                    Address = AddressEntry.Text.Trim(),
                    Latitude = _latitude,
                    Longitude = _longitude
                };

                var res = await _api.PostMultipartAsync<ReqInsertFoodBank, ResBase>(
                    "foodBank/insert",
                    req,
                    _logoFileResult,
                    "LogoImage");

                if (res?.Success == true)
                {
                    await DisplayAlert("�xito", "Banco de alimentos insertado correctamente", "OK");
                    isSuccess = true;
                }
                else
                {
                    var errs = res?.Errors?.Select(x => x.Description) ?? new[] { "Desconocido" };
                    await DisplayAlert("Error", "No se pudo insertar: " + string.Join(", ", errs), "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error al insertar: " + ex.Message, "OK");
            }
            finally
            {
                await Navigation.PopModalAsync(); // Cierra el loader
                if (isSuccess)
                {
                    await Navigation.PopAsync(); // Cierra la pantalla de inserci�n si todo sali� bien
                }
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e) =>
            _ = Navigation.PopAsync();
    }
}
