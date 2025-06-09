using Microsoft.Maui.Controls;
using SaborSostenibleFrontEnd.Entities;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace SaborSostenibleFrontEnd
{
    public partial class BuySupriseBagPage : ContentPage
    {
        private Restaurante _restaurante;
        private string _orderCode;

        public BuySupriseBagPage(Restaurante restaurante)
        {
            InitializeComponent();
            _restaurante = restaurante;
            _orderCode = GenerateOrderCode();

            RestauranteNombreLabel.Text = _restaurante.nombreRestaurante;
            RestauranteDescripcionLabel.Text = _restaurante.descripcionRestaurante;
            RestauranteImagen.Source = _restaurante.imagen;
            RestauranteTelefonoLabel.Text = _restaurante.telefono;
            SinpeDetalleLabel.Text = $"Pago bolsa sorpresa {_restaurante.nombreRestaurante}-{_orderCode} {_restaurante.telefono}";
        }

        private string GenerateOrderCode()
        {
            Random random = new Random();
            return $"ORD{random.Next(100000, 999999)}";
        }

        private async void OnProcederCompraClicked(object sender, EventArgs e)
        {
            // Eliminamos la validación del teléfono
            await CompraStep.FadeTo(0, 200);
            CompraStep.IsVisible = false;

            InstruccionesStep.Opacity = 0;
            InstruccionesStep.IsVisible = true;
            await InstruccionesStep.FadeTo(1, 200);

            Paso1Icon.BackgroundColor = Colors.LightGray;
            Paso2Icon.BackgroundColor = Application.Current.Resources["Primary"] as Color;
        }

        private async void OnCopiarDetalleClicked(object sender, EventArgs e)
        {
            try
            {
                await Clipboard.Default.SetTextAsync(SinpeDetalleLabel.Text);
                await DisplayAlert("Copiado", "Detalle de SINPE copiado al portapapeles", "OK");
            }
            catch (Exception)
            {
                await DisplayAlert("Error", "No se pudo copiar el texto al portapapeles", "OK");
            }
        }

        private async void OnFinalizarClicked(object sender, EventArgs e)
        {
            await InstruccionesStep.FadeTo(0, 200);
            InstruccionesStep.IsVisible = false;

            ConfirmacionStep.Opacity = 0;
            ConfirmacionStep.IsVisible = true;
            await ConfirmacionStep.FadeTo(1, 200);

            OrdenFinalLabel.Text = _orderCode;
            TelefonoFinalLabel.Text = _restaurante.telefono;

            Paso2Icon.BackgroundColor = Colors.LightGray;
            Paso3Icon.BackgroundColor = Application.Current.Resources["Primary"] as Color;

            // ?? Hacer POST a /api/order/insert
            await InsertarOrdenAsync();
        }

        private async Task InsertarOrdenAsync()
        {
            try
            {
                using var httpClient = new HttpClient();

                var payload = new
                {
                    BagsIdsVarchar = _orderCode,
                    BusinessId = _restaurante.idRestaurante,
                    IsDonation = false,
                    FoodBankId = 1 // Esto puedes ajustarlo si viene del restaurante o es fijo
                };

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("http://34.39.128.125/api/order/insert", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"No se pudo registrar la orden: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error inesperado al registrar la orden: {ex.Message}", "OK");
            }
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert("Confirmar", "¿Deseas regresar?", "Sí", "No");
            if (result)
            {
                await Navigation.PopAsync();
            }
        }

        private async void OnVolverInicioClicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            return true; // Desactiva botón físico de retroceso
        }
    }
}
