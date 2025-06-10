using Microsoft.Maui.Controls;
using SaborSostenibleFrontEnd.Entities;
using System.Text;
using System.Text.Json;

namespace SaborSostenibleFrontEnd;

public partial class BuySupriseBagPage : ContentPage
{
    private readonly Restaurante _restaurante;
    private readonly SurpriseBag _bolsa;
    private int _orderId = 0;
    private List<(int Id, string Name)> _foodBanks = new();
    private string _orderCode = "";

    public BuySupriseBagPage(Restaurante restaurante, SurpriseBag bolsa)
    {
        InitializeComponent();
        _restaurante = restaurante;
        _bolsa = bolsa;

        _orderCode = $"ORD{new Random().Next(100000, 999999)}";

        RestauranteNombreLabel.Text = _restaurante.nombreRestaurante;
        RestauranteDescripcionLabel.Text = _restaurante.descripcionRestaurante;
        RestauranteImagen.Source = _restaurante.imagen;
        RestauranteTelefonoLabel.Text = _restaurante.telefono;

        DescripcionBolsaLabel.Text = _bolsa.Description;
        PrecioBolsaLabel.Text = $"Precio: ₡{_bolsa.Price:N2}";

        SinpeDetalleLabel.Text = $"Pago bolsa sorpresa {_restaurante.nombreRestaurante}-{_orderCode} {_restaurante.telefono}";
        PagoMontoLabel.Text = $"₡{_bolsa.Price:N0}";
        PagoTelefonoLabel.Text = _restaurante.telefono;

        _ = CargarFoodBanksAsync();
    }

    private void OnDonacionToggled(object sender, ToggledEventArgs e)
    {
        FoodBankPicker.IsVisible = e.Value;
    }

    private async Task CargarFoodBanksAsync()
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync("http://34.39.128.125/api/activeFoodBanksDetails/get");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("FoodBanks", out var banks))
                {
                    _foodBanks.Clear();
                    FoodBankPicker.Items.Clear();

                    foreach (var fb in banks.EnumerateArray())
                    {
                        var name = fb.GetProperty("Name").GetString();
                        var id = fb.GetProperty("FoodBankId").GetInt32();
                        _foodBanks.Add((id, name));
                        FoodBankPicker.Items.Add(name);
                    }
                }
            }
        }
        catch
        {
            await DisplayAlert("Error", "No se pudieron cargar los bancos de comida", "OK");
        }
    }

    private async void OnProcederCompraClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CustomerPhoneEntry.Text))
        {
            await DisplayAlert("Error", "Por favor ingresa tu número de teléfono", "OK");
            return;
        }

        int? foodBankId = null;

        if (DonacionSwitch.IsToggled)
        {
            if (FoodBankPicker.SelectedIndex < 0)
            {
                await DisplayAlert("Error", "Selecciona un banco de comida para donar", "OK");
                return;
            }

            foodBankId = _foodBanks[FoodBankPicker.SelectedIndex].Id;
        }

        var payload = new
        {
            BagsIdsVarchar = _bolsa.Id,
            BusinessId = _restaurante.idRestaurante,
            IsDonation = DonacionSwitch.IsToggled,
            FoodBankId = foodBankId ?? null
        };

        try
        {
            using var client = new HttpClient();
            var token = Preferences.Get("SessionId", null);
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://34.39.128.125/api/order/insert", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseJson);
                _orderId = doc.RootElement.GetProperty("OrderId").GetInt32();
                Paso1Icon.BackgroundColor = Colors.LightGray;
                Paso2Icon.BackgroundColor = Color.FromArgb("#2E7D32");


                await MostrarPaso2Async();
            }
            else
            {
                await DisplayAlert("Error", "No se pudo registrar la orden", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
        }
    }

    private async Task MostrarPaso2Async()
    {
        try
        {
            var payload = new { OrderId = _orderId.ToString() };
            var json = JsonSerializer.Serialize(payload);
            using var client = new HttpClient();
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var token = Preferences.Get("SessionId", null);
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync("http://34.39.128.125/api/customerPurchaseInstructions/post", content);

            if (response.IsSuccessStatusCode)
            {
                var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var info = doc.RootElement.GetProperty("PurchaseInstructions");

                PagoTelefonoLabel.Text = info.GetProperty("PhoneNumberBusiness").GetString();
                PagoMontoLabel.Text = $"₡{_bolsa.Price:N0}";
                SinpeDetalleLabel.Text = $"Pago bolsa sorpresa {_restaurante.nombreRestaurante}-{_orderCode} {_restaurante.telefono}";

                await Paso1.FadeTo(0, 200);
                Paso1.IsVisible = false;
                InstruccionesStep.Opacity = 0;
                InstruccionesStep.IsVisible = true;
                await InstruccionesStep.FadeTo(1, 200);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al obtener instrucciones de pago: {ex.Message}", "OK");
        }
    }

    private async void OnFinalizarClicked(object sender, EventArgs e)
    {
        try
        {
            Paso2Icon.BackgroundColor = Colors.LightGray;
            Paso3Icon.BackgroundColor = Color.FromArgb("#2E7D32");

            var json = JsonSerializer.Serialize(new { OrderId = _orderId });
            using var client = new HttpClient();
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var token = Preferences.Get("SessionId", null);
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync("http://34.39.128.125/api/customerFinalPaymentData/post", content);

            if (response.IsSuccessStatusCode)
            {
                var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var data = doc.RootElement.GetProperty("FinalPaymentData");

                OrdenFinalLabel.Text = $"Código: {data.GetProperty("OrderCode").GetString()}";
                TelefonoFinalLabel.Text = $"Teléfono: {data.GetProperty("PhoneNumberBusiness").GetString()}";

                await InstruccionesStep.FadeTo(0, 200);
                InstruccionesStep.IsVisible = false;
                ConfirmacionStep.Opacity = 0;
                ConfirmacionStep.IsVisible = true;
                await ConfirmacionStep.FadeTo(1, 200);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al confirmar el pago: {ex.Message}", "OK");
        }
    }

    private async void OnCopiarDetalleClicked(object sender, EventArgs e)
    {
        try
        {
            await Clipboard.Default.SetTextAsync(SinpeDetalleLabel.Text);
            await DisplayAlert("Copiado", "Detalle de SINPE copiado al portapapeles", "OK");
        }
        catch
        {
            await DisplayAlert("Error", "No se pudo copiar el texto al portapapeles", "OK");
        }
    }

    private async void OnVolverInicioClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    protected override bool OnBackButtonPressed() => true;
}
