using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.VisualBasic;
using SaborSostenibleFrontEnd.Entities;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.AdminPages
{
    public partial class InsertBusinessPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private List<UserByCustomerRole> _users;
        private int _selectedAdminUserId;
        private FileResult _logoFileResult;
        private decimal _latitude;
        private decimal _longitude;

        public InsertBusinessPage()
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
                    var errors = resp?.Errors?.Select(e => e.Description) ?? new[] { "Error desconocido" };
                    await DisplayAlert("Error", "No se pudieron cargar usuarios: " + string.Join(", ", errors), "OK");
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
            {
                var u = _users[AdminPicker.SelectedIndex];
                _selectedAdminUserId = u.UserId;
            }
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

        private void MostrarError(string msg) =>
            _ = DisplayAlert("Error", msg, "OK");

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text)
             || string.IsNullOrWhiteSpace(DescriptionEntry.Text)
             || _selectedAdminUserId == 0
             || string.IsNullOrWhiteSpace(EmailEntry.Text)
             || string.IsNullOrWhiteSpace(PhoneEntry.Text)
             || string.IsNullOrWhiteSpace(AddressEntry.Text)
             || _logoFileResult == null)
            {
                MostrarError("Complete todos los campos y seleccione un logo");
                return false;
            }

            if (!EsEmailValido(EmailEntry.Text))
            {
                MostrarError("Correo no válido");
                return false;
            }

            if (_latitude == 0 || _longitude == 0)
            {
                DisplayAlert("Error", "Debe seleccionar una ubicación en el mapa", "OK");
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
                    UbicacionLabel.Text = $"Ubicación elegida: {_latitude:F5}, {_longitude:F5}";
                });
            }));
        }
        private async void OnInsertClicked(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;

            // muestra loader si lo tienes, o comenta estas dos líneas
            var loader = new LoadingPage();
            await Navigation.PushModalAsync(loader);

            bool isSuccess = false;

            try
            {
                var req = new ReqInsertBusiness
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

                var res = await _api.PostMultipartAsync<ReqInsertBusiness, ResBase>(
                    "business/insert",
                    req,
                    _logoFileResult,
                    "LogoImage");

                if (res != null && res.Success)
                {
                    await DisplayAlert("Éxito", "Negocio insertado correctamente", "OK");
                    isSuccess = true;
                }
                else
                {
                    var errs = res?.Errors?.Select(x => x.Description) ?? new[] { "Desconocido" };
                    MostrarError("Error al insertar: " + string.Join(", ", errs));
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al insertar negocio: " + ex.Message);
            }
            finally
            {
                await Navigation.PopModalAsync(); // Cierra el loader
                if (isSuccess)
                {
                    await Navigation.PopAsync(); // Cierra la pantalla de inserción si todo salió bien
                }
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e) =>
            _ = Navigation.PopAsync();
    }
}
