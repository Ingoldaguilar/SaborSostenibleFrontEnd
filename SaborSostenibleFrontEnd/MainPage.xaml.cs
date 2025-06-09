using Microsoft.Maui.Controls;
using SaborSostenibleFrontEnd.Entities;
using System.Collections.ObjectModel;
using System.Net.Http;
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

            _ = CargarSaludoPersonalizadoAsync();
            _ = CargarRestaurantesDesdeApiAsync();
            _ = CargarPedidosAsync();
            _ = MostrarDatosUsuarioAsync();
        }


        private async Task CargarRestaurantesDesdeApiAsync()
        {
            try
            {
                var token = Preferences.Get("SessionId", null);
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Error", "No hay sesión activa", "OK");
                    return;
                }

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync("http://34.39.128.125/api/activeBusinessesDetails/get");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("Businesses", out JsonElement empresas))
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
                        await DisplayAlert("Info", "No se encontraron restaurantes disponibles.", "OK");
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Error al obtener restaurantes: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Excepción: {ex.Message}", "OK");
            }
        }


        private async void OnComprarClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Restaurante restaurante)
            {
                await Navigation.PushAsync(new BuySupriseBagPage(restaurante));
            }
        }

        private async Task MostrarDatosUsuarioAsync()
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
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await client.GetAsync("http://34.39.128.125/api/userGreetingInfo/get");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(content);

                    if (json.RootElement.TryGetProperty("GreetingInfo", out JsonElement info))
                    {
                        string Safe(string key) =>
                            info.TryGetProperty(key, out var val) && !string.IsNullOrWhiteSpace(val.GetString()) ? val.GetString() : "N/A";

                        string fullName = $"{Safe("FirstName1")} {Safe("FirstName2")} {Safe("LastName1")} {Safe("LastName2")}".Trim();
                        UsuarioNombreLabel.Text = string.IsNullOrWhiteSpace(fullName) ? "N/A" : fullName;

                        UsuarioEmailLabel.Text = Safe("Email");
                        UsuarioTelefonoLabel.Text = Safe("PhoneNumber");
                        UsuarioDireccionLabel.Text = Safe("Address");
                        UsuarioRolLabel.Text = "Usuario";
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se encontró la información del usuario", "OK");
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Error al obtener perfil: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Excepción: {ex.Message}", "OK");
            }
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert("Cerrar Sesión", "¿Estás seguro que deseas cerrar sesión?", "Sí", "No");
            if (confirm)
            {
                Preferences.Remove("SessionId");
                Preferences.Remove("UserEmail");
                Preferences.Remove("UserName");

                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }


        private async Task CargarPedidosAsync()
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
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await client.GetAsync("http://34.39.128.125/api/orders/by-user");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("Orders", out JsonElement ordenes))
                    {
                        Pedidos.Clear();

                        foreach (var item in ordenes.EnumerateArray())
                        {
                            Pedidos.Add(new Pedido
                            {
                                OrderId = item.GetProperty("OrderId").GetInt32(),
                                RestaurantName = item.GetProperty("RestaurantName").GetString(),
                                Date = DateTime.Parse(item.GetProperty("Date").GetString()).ToString("dd/MM/yyyy HH:mm"),
                                Status = item.GetProperty("Status").GetString()
                            });
                        }
                    }
                    else
                    {
                        await DisplayAlert("Atención", "No hay pedidos aún.", "OK");
                    }
                }
                else
                {
                    var body = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"No se pudieron obtener los pedidos: {body}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
            }
        }

        private async void OnSolicitarVoluntariadoClicked(object sender, EventArgs e)
        {
            var confirmar = await DisplayAlert("Solicitud de Voluntariado",
                "¿Deseas solicitar ser voluntario en Sabor Sostenible?", "Sí", "No");

            if (!confirmar) return;

            try
            {
                var token = Preferences.Get("SessionId", null);
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Error", "No hay sesión activa", "OK");
                    return;
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

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
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer " + token);

                var response = await client.GetAsync("http://34.39.128.125/api/userGreetingInfo/get");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(content);
                    if (json.RootElement.TryGetProperty("GreetingInfo", out JsonElement info))
                    {
                        var nombre = info.GetProperty("FirstName1").GetString();
                        SaludoLabel.Text = $"¡Hola, {nombre}! 👋";
                    }
                    else
                    {
                        SaludoLabel.Text = "¡Hola! 👋";
                    }
                }
                else
                {
                    SaludoLabel.Text = "¡Hola! 👋";
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
                await Navigation.PushAsync(new RestauranteDetallePage(restaurante));
            }
        }

    }
}
