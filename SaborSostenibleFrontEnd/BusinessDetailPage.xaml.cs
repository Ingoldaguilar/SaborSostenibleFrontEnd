using SaborSostenibleFrontEnd.Entities;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace SaborSostenibleFrontEnd;

public partial class BusinessDetailPage : ContentPage
{
    private readonly Restaurante _restaurante;
    public ObservableCollection<SurpriseBag> BolsasDisponibles { get; set; } = new();

    public BusinessDetailPage(Restaurante restaurante)
    {
        InitializeComponent();
        _restaurante = restaurante;
        BindingContext = this;

        MostrarInformacion();
        _ = CargarBolsasDisponiblesAsync();
    }

    private void MostrarInformacion()
    {
        RestauranteNombreLabel.Text = _restaurante.nombreRestaurante;
        RestauranteDescripcionLabel.Text = _restaurante.descripcionRestaurante;
        RestauranteTelefonoLabel.Text = _restaurante.telefono;
        RestauranteImagen.Source = _restaurante.imagen;
    }

    private async Task CargarBolsasDisponiblesAsync()
    {
        try
        {
            var token = Preferences.Get("SessionId", null);
            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Error", "No hay sesión activa", "OK");
                return;
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new { BusinessId = _restaurante.idRestaurante };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://34.39.128.125/api/surpriseBagsAvailableForCustomers/post", content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"No autorizado: {error}", "OK");
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("SurpriseBagAvailableForCustomers", out JsonElement bolsas))
            {
                BolsasDisponibles.Clear();
                foreach (var item in bolsas.EnumerateArray())
                {
                    BolsasDisponibles.Add(new SurpriseBag
                    {
                        Description = item.GetProperty("Description").GetString(),
                        Price = item.GetProperty("Price").GetDecimal()
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudieron cargar las bolsas sorpresa: " + ex.Message, "OK");
        }
    }

    private async void OnComprarClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is SurpriseBag bolsa)
        {
            await Navigation.PushAsync(new BuySupriseBagPage(_restaurante, bolsa));
        }
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
