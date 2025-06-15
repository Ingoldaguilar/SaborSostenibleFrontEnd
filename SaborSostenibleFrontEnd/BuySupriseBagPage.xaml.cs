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
    private const string BASE_URL = "http://34.39.128.125/api";
    private int _pasoActual = 1; // Variable para controlar el paso actual

    public BuySupriseBagPage(Restaurante restaurante, SurpriseBag bolsa)
    {
        InitializeComponent();
        _restaurante = restaurante;
        _bolsa = bolsa;

        _orderCode = $"ORD{new Random().Next(100000, 999999)}";

        InicializarInterfaz();
        _ = CargarFoodBanksAsync();

        NavigationPage.SetHasNavigationBar(this, false);
        Padding = new Thickness(0);

        // Inicializar la visualización de pasos
        ActualizarVisualizacionPasos();
    }

    private void InicializarInterfaz()
    {
        // Información del restaurante
        RestauranteNombreLabel.Text = _restaurante.nombreRestaurante;
        RestauranteDescripcionLabel.Text = _restaurante.descripcionRestaurante;
        RestauranteTelefonoLabel.Text = _restaurante.telefono;

        // Información de la bolsa
        DescripcionBolsaLabel.Text = _bolsa.Description;
        PrecioBolsaLabel.Text = $"₡{_bolsa.Price:N0}";

        // Preparar información de pago
        SinpeDetalleLabel.Text = $"Pago bolsa sorpresa {_restaurante.nombreRestaurante}-{_orderCode}";
        PagoMontoLabel.Text = $"₡{_bolsa.Price:N0}";
        PagoTelefonoLabel.Text = _restaurante.telefono;

        HeaderPaso1.IsVisible = true;
    }

    private void ActualizarVisualizacionPasos()
    {
        // Reset todos los pasos a estado inactivo
        ResetearPasos();

        switch (_pasoActual)
        {
            case 1:
                // Paso 1 activo (verde)
                Paso1Icon.BackgroundColor = Color.FromArgb("#789262");
                Paso1Label.TextColor = Colors.White;
                EtiquetaPaso1.TextColor = Color.FromArgb("#789262");
                EtiquetaPaso1.FontAttributes = FontAttributes.Bold;
                break;

            case 2:
                // Paso 1 completado (verde más claro)
                Paso1Icon.BackgroundColor = Color.FromArgb("#4CAF50");
                Paso1Label.TextColor = Colors.White;
                LineaPaso1.BackgroundColor = Color.FromArgb("#4CAF50");
                EtiquetaPaso1.TextColor = Color.FromArgb("#4CAF50");
                EtiquetaPaso1.FontAttributes = FontAttributes.Bold;

                // Paso 2 activo (verde del header)
                Paso2Icon.BackgroundColor = Color.FromArgb("#789262");
                Paso2Label.TextColor = Colors.White;
                EtiquetaPaso2.TextColor = Color.FromArgb("#789262");
                EtiquetaPaso2.FontAttributes = FontAttributes.Bold;
                break;

            case 3:
                // Pasos 1 y 2 completados
                Paso1Icon.BackgroundColor = Color.FromArgb("#4CAF50");
                Paso1Label.TextColor = Colors.White;
                LineaPaso1.BackgroundColor = Color.FromArgb("#4CAF50");
                EtiquetaPaso1.TextColor = Color.FromArgb("#4CAF50");
                EtiquetaPaso1.FontAttributes = FontAttributes.Bold;

                Paso2Icon.BackgroundColor = Color.FromArgb("#4CAF50");
                Paso2Label.TextColor = Colors.White;
                LineaPaso2.BackgroundColor = Color.FromArgb("#4CAF50");
                EtiquetaPaso2.TextColor = Color.FromArgb("#4CAF50");
                EtiquetaPaso2.FontAttributes = FontAttributes.Bold;

                // Paso 3 activo
                Paso3Icon.BackgroundColor = Color.FromArgb("#789262");
                Paso3Label.TextColor = Colors.White;
                EtiquetaPaso3.TextColor = Color.FromArgb("#789262");
                EtiquetaPaso3.FontAttributes = FontAttributes.Bold;
                break;
        }
    }

    private void ResetearPasos()
    {
        // Pasos inactivos: fondo blanco, número negro
        Paso1Icon.BackgroundColor = Colors.White;
        Paso1Label.TextColor = Colors.Black;
        Paso2Icon.BackgroundColor = Colors.White;
        Paso2Label.TextColor = Colors.Black;
        Paso3Icon.BackgroundColor = Colors.White;
        Paso3Label.TextColor = Colors.Black;

        // Líneas grises
        LineaPaso1.BackgroundColor = Color.FromArgb("#E0E0E0");
        LineaPaso2.BackgroundColor = Color.FromArgb("#E0E0E0");

        // Etiquetas grises
        EtiquetaPaso1.TextColor = Color.FromArgb("#757575");
        EtiquetaPaso1.FontAttributes = FontAttributes.None;
        EtiquetaPaso2.TextColor = Color.FromArgb("#757575");
        EtiquetaPaso2.FontAttributes = FontAttributes.None;
        EtiquetaPaso3.TextColor = Color.FromArgb("#757575");
        EtiquetaPaso3.FontAttributes = FontAttributes.None;
    }

    private void OnDonacionToggled(object sender, ToggledEventArgs e)
    {
        FoodBankFrame.IsVisible = e.Value;

        // Animación suave para mostrar/ocultar
        if (e.Value)
        {
            FoodBankFrame.Opacity = 0;
            FoodBankFrame.FadeTo(1, 300, Easing.CubicOut);
        }
    }

    private async Task CargarFoodBanksAsync()
    {
        try
        {
            using var client = new HttpClient();
            var token = Preferences.Get("SessionId", null);
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{BASE_URL}/allFoodBanksDetails/get");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("Success", out var success) && success.GetBoolean())
                {
                    if (doc.RootElement.TryGetProperty("FoodBanks", out var banks))
                    {
                        _foodBanks.Clear();
                        FoodBankPicker.Items.Clear();

                        foreach (var fb in banks.EnumerateArray())
                        {
                            // Verificar si está activo
                            if (fb.TryGetProperty("IsActive", out var isActive) && isActive.GetBoolean())
                            {
                                var name = fb.GetProperty("Name").GetString();
                                var id = fb.GetProperty("FoodBankId").GetInt32();
                                _foodBanks.Add((id, name));
                                FoodBankPicker.Items.Add(name);
                            }
                        }
                    }
                }
                else
                {
                    if (doc.RootElement.TryGetProperty("Errors", out var errors))
                    {
                        var errorMessages = errors.EnumerateArray()
                            .Select(e => e.GetProperty("Description").GetString())
                            .ToList();
                        await DisplayAlert("Error", string.Join(", ", errorMessages), "OK");
                    }
                }
            }
            else
            {
                await DisplayAlert("Error", "No se pudieron cargar los bancos de comida", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar bancos de comida: {ex.Message}", "OK");
        }
    }

    private async void OnProcederCompraClicked(object sender, EventArgs e)
    {
        // Validar selección de banco de comida si es donación
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

        // Preparar payload para crear orden
        var payload = new
        {
            BagsIdsVarchar = _bolsa.Id,
            BusinessId = _restaurante.idRestaurante,
            IsDonation = DonacionSwitch.IsToggled,
            FoodBankId = foodBankId
        };

        try
        {
            using var client = new HttpClient();
            var token = Preferences.Get("SessionId", null);
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BASE_URL}/order/insert", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseJson);

                if (doc.RootElement.TryGetProperty("Success", out var success) && success.GetBoolean())
                {
                    _orderId = doc.RootElement.GetProperty("OrderId").GetInt32();

                    // Actualizar al paso 2
                    _pasoActual = 2;
                    ActualizarVisualizacionPasos();
                    await MostrarPaso2Async();
                }
                else
                {
                    if (doc.RootElement.TryGetProperty("Errors", out var errors))
                    {
                        var errorMessages = errors.EnumerateArray()
                            .Select(e => e.GetProperty("Description").GetString())
                            .ToList();
                        await DisplayAlert("Error", string.Join(", ", errorMessages), "OK");
                    }
                }
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
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync($"{BASE_URL}/customerPurchaseInstructions/post", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseJson);

                if (doc.RootElement.TryGetProperty("Success", out var success) && success.GetBoolean())
                {
                    var info = doc.RootElement.GetProperty("PurchaseInstructions");

                    // Actualizar información de pago
                    PagoTelefonoLabel.Text = info.GetProperty("PhoneNumberBusiness").GetString();
                    PagoMontoLabel.Text = $"₡{_bolsa.Price:N0}";
                    var businessName = info.GetProperty("NameBusiness").GetString();
                    var orderCode = info.GetProperty("OrderCode").GetString();
                    SinpeDetalleLabel.Text = $"Pago bolsa sorpresa {businessName}-{orderCode}";

                    // Transición animada entre pasos
                    await Paso1.FadeTo(0, 300);
                    Paso1.IsVisible = false;
                    HeaderPaso1.IsVisible = false;

                    InstruccionesStep.Opacity = 0;
                    InstruccionesStep.IsVisible = true;
                    await InstruccionesStep.FadeTo(1, 300);
                }
                else
                {
                    if (doc.RootElement.TryGetProperty("Errors", out var errors))
                    {
                        var errorMessages = errors.EnumerateArray()
                            .Select(e => e.GetProperty("Description").GetString())
                            .ToList();
                        await DisplayAlert("Error", string.Join(", ", errorMessages), "OK");
                    }
                }
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
            // Actualizar al paso 3
            _pasoActual = 3;
            ActualizarVisualizacionPasos();

            var json = JsonSerializer.Serialize(new { OrderId = _orderId });
            using var client = new HttpClient();
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var token = Preferences.Get("SessionId", null);
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync($"{BASE_URL}/customerFinalPaymentData/post", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseJson);

                if (doc.RootElement.TryGetProperty("Success", out var success) && success.GetBoolean())
                {
                    var data = doc.RootElement.GetProperty("FinalPaymentData");

                    // Actualizar información final
                    OrdenFinalLabel.Text = $"Código: {data.GetProperty("OrderCode").GetString()}";
                    TelefonoFinalLabel.Text = $"Negocio: {data.GetProperty("NameBusiness").GetString()}";
                    EstadoFinalLabel.Text = $"Estado: {data.GetProperty("State").GetString()}";

                    // Transición animada al paso final
                    HeaderPaso1.IsVisible = false;
                    await InstruccionesStep.FadeTo(0, 300);
                    InstruccionesStep.IsVisible = false;

                    ConfirmacionStep.Opacity = 0;
                    ConfirmacionStep.IsVisible = true;
                    await ConfirmacionStep.FadeTo(1, 300);
                }
                else
                {
                    if (doc.RootElement.TryGetProperty("Errors", out var errors))
                    {
                        var errorMessages = errors.EnumerateArray()
                            .Select(e => e.GetProperty("Description").GetString())
                            .ToList();
                        await DisplayAlert("Error", string.Join(", ", errorMessages), "OK");
                    }
                }
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

            // Toast personalizado
            var button = sender as Button;
            var originalText = button.Text;
            button.Text = "✓";
            button.BackgroundColor = Color.FromArgb("#4CAF50");

            await Task.Delay(1000);

            button.Text = originalText;
            button.BackgroundColor = Color.FromArgb("#2E7D32");
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

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("¿Deseas salir?", "Volver cancelará esta compra", "Sí", "No");
        if (confirm)
            await Navigation.PopAsync();
    }
}