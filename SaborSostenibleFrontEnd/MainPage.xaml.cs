using Microsoft.Maui.Controls;
using SaborSostenibleFrontEnd.Entities;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd
{
    public partial class MainPage : TabbedPage
    {
        public ObservableCollection<Restaurante> Restaurantes { get; set; } = new();
        public ObservableCollection<Pedido> Pedidos { get; set; } = new();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            NavigationPage.SetHasNavigationBar(this, false);
            Padding = new Thickness(0);

            // Suscribirse al evento de cambio de tab
            this.CurrentPageChanged += OnCurrentPageChanged;

            _ = CargarSaludoPersonalizadoAsync();
            _ = CargarRestaurantesDesdeApiAsync();
            _ = CargarPedidosAsync();
            _ = MostrarDatosUsuarioAsync();
        }

        private async void OnCurrentPageChanged(object sender, EventArgs e)
        {
            // Verificar si el tab actual es el de "Pedidos"
            if (this.CurrentPage != null && this.CurrentPage.Title == "Pedidos")
            {
                await CargarPedidosAsync();
            }
        }

        private async Task CargarRestaurantesDesdeApiAsync(string searchText = null)
        {
            try
            {
                using var httpClient = new HttpClient();
                var token = Preferences.Get("SessionId", null);
                if (!string.IsNullOrEmpty(token))
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    response = await httpClient.GetAsync("http://34.39.128.125/api/activeBusinessesDetails/get");
                }
                else
                {
                    var content = new StringContent(JsonSerializer.Serialize(new { SearchText = searchText }), Encoding.UTF8, "application/json");
                    response = await httpClient.PostAsync("http://34.39.128.125/api/searchBusinesses/post", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("Businesses", out var empresas))
                    {
                        Restaurantes.Clear();
                        foreach (var item in empresas.EnumerateArray())
                        {
                            Restaurantes.Add(new Restaurante
                            {
                                idRestaurante = item.GetProperty("BusinessId").GetInt32(),
                                nombreRestaurante = item.GetProperty("Name").GetString(),
                                descripcionRestaurante = item.GetProperty("Description").GetString(),
                                telefono = item.GetProperty("PhoneNumber").GetString(),
                                imagen = "http://34.39.128.125/" + item.GetProperty("LogoImage").GetString()
                            });
                        }
                    }
                    else
                    {
                        Restaurantes.Clear();
                    }
                }
                else
                {
                    Restaurantes.Clear();
                }
            }
            catch
            {
                Restaurantes.Clear();
            }
        }

        private async Task CargarPedidosAsync()
        {
            try
            {
                Pedidos.Clear();
                var token = Preferences.Get("SessionId", null);
                if (string.IsNullOrEmpty(token)) return;

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync("http://34.39.128.125/api/userOrders/get");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("OrdersByUserId", out var ordenes))
                    {
                        foreach (var item in ordenes.EnumerateArray())
                        {
                            Pedidos.Add(new Pedido
                            {
                                OrderId = item.GetProperty("OrderId").GetInt32(),
                                RestaurantName = item.GetProperty("BusinessName").GetString(),
                                Date = DateTime.Parse(item.GetProperty("OrderDate").GetString()).ToString("dd/MM/yyyy HH:mm"),
                                Status = item.GetProperty("State").GetString(),
                                Total = item.GetProperty("TotalAmount").GetDecimal(),
                                Bags = item.GetProperty("BagsQuantity").GetInt32()
                            });
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No se pudieron obtener los pedidos.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ha ocurrido un error al cargar pedidos.\n\n{ex.Message}", "OK");
            }
        }

        private async void OnSearchCompleted(object sender, EventArgs e)
        {
            if (sender is Entry entry)
            {
                var texto = entry.Text?.Trim();
                await CargarRestaurantesDesdeApiAsync(string.IsNullOrEmpty(texto) ? null : texto);
            }
        }

        private async Task MostrarDatosUsuarioAsync()
        {
            try
            {
                var token = Preferences.Get("SessionId", null);
                if (string.IsNullOrEmpty(token)) return;

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync("http://34.39.128.125/api/userGreetingInfo/get");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("GreetingInfo", out var info))
                    {
                        string Safe(string key) =>
                            info.TryGetProperty(key, out var val) && !string.IsNullOrWhiteSpace(val.GetString()) ? val.GetString() : null;

                        // Nombre completo
                        string[] nombreComponentes = { Safe("FirstName1"), Safe("FirstName2"), Safe("LastName1"), Safe("LastName2") };
                        UsuarioNombreLabel.Text = string.Join(" ", nombreComponentes.Where(s => !string.IsNullOrWhiteSpace(s)));

                        UsuarioEmailLabel.Text = Safe("Email") ?? "N/A";
                        UsuarioTelefonoLabel.Text = Safe("PhoneNumber") ?? "N/A";
                        UsuarioDireccionLabel.Text = Safe("Address") ?? "N/A";
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al obtener datos del usuario.\n\n{ex.Message}", "OK");
            }
        }

        private async Task CargarSaludoPersonalizadoAsync()
        {
            try
            {
                var token = Preferences.Get("SessionId", null);
                if (string.IsNullOrEmpty(token))
                {
                    SaludoLabel.Text = "¡Hola! 👋";
                    return;
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync("http://34.39.128.125/api/userGreetingInfo/get");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("GreetingInfo", out var info))
                    {
                        var nombre = info.GetProperty("FirstName1").GetString();
                        SaludoLabel.Text = $"¡Hola, {nombre}! 👋";
                    }
                }
            }
            catch
            {
                SaludoLabel.Text = "¡Hola! 👋";
            }
        }

        private async void OnRestauranteCardTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is Restaurante restaurante)
            {
                await Navigation.PushAsync(new BusinessDetailPage(restaurante));
            }
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Cerrar Sesión", "¿Estás seguro que deseas cerrar sesión?", "Sí", "No");
            if (!confirm) return;

            Preferences.Clear();
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }

        private async void OnSolicitarVoluntariadoClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Solicitud de Voluntariado", "¿Deseas solicitar ser voluntario en Sabor Sostenible?", "Sí", "No");
            if (!confirm) return;

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

                var response = await client.PostAsync("http://34.39.128.125/api/createVolunteerRequest/post", null);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Tu solicitud de voluntariado ha sido enviada.", "OK");
                }
                else
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"No se pudo enviar la solicitud: {msg}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
            }
        }

        private async void OnDonacionesPendientesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SaborSostenibleFrontEnd.VolunteerPages.PendingDonationsPage());
        }

        // Método para limpiar recursos cuando se destruye la página
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.CurrentPageChanged -= OnCurrentPageChanged;
        }

        // Método para configurar la visibilidad de los botones según el rol del usuario
        private void ConfigurarBotonesPorRol()
        {
            // Obtener el rol del usuario desde las preferencias
            string userRole = Preferences.Get("UserRole", string.Empty);

            // Ocultar ambos botones por defecto
            SolicitarVoluntarioButton.IsVisible = false;
            DonacionesPendientesButton.IsVisible = false;

            // Mostrar el botón correspondiente según el rol
            switch (userRole?.ToLower())
            {
                case "customer":
                    SolicitarVoluntarioButton.IsVisible = true;
                    break;
                case "volunteer":
                    DonacionesPendientesButton.IsVisible = true;
                    break;
                default:
                    // Si no hay rol o es desconocido, no mostrar ningún botón especial
                    break;
            }
        }

        // Llamar este método en el constructor o en OnAppearing
        protected override void OnAppearing()
        {
            base.OnAppearing();
            ConfigurarBotonesPorRol();
            // ... resto de tu código de OnAppearing
        }
    }
}