using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SaborSostenibleFrontEnd.Security;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://34.39.128.125/api/";

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    // Método para configurar el Bearer token
    private void SetBearerToken()
    {
        var sessionId = Preferences.Get("SessionId", string.Empty);
        if (!string.IsNullOrEmpty(sessionId))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", sessionId);
        }
    }

    // GET genérico con autenticación
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            SetBearerToken();
            var response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorized();
                return default(T);
            }
            else
            {
                throw new HttpRequestException($"Error en API: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error en GET {endpoint}: {ex.Message}");
        }
    }

    // POST genérico con autenticación
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            SetBearerToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorized();
                return default(TResponse);
            }
            else
            {
                throw new HttpRequestException($"Error en API: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error en POST {endpoint}: {ex.Message}");
        }
    }

    // POST sin respuesta (solo éxito/error)
    public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest data)
    {
        try
        {
            SetBearerToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorized();
                return false;
            }
            else
            {
                throw new HttpRequestException($"Error en API: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error en POST {endpoint}: {ex.Message}");
        }
    }

    // Manejar token expirado
    private async Task HandleUnauthorized()
    {
        // Limpiar datos de sesión
        Preferences.Remove("SessionId");
        Preferences.Remove("UserEmail");
        Preferences.Set("IsLoggedIn", false);

        // Mostrar mensaje y redirigir al login
        await Application.Current.MainPage.DisplayAlert("Sesión expirada",
            "Su sesión ha expirado. Por favor, inicie sesión nuevamente.", "OK");

        Application.Current.MainPage = new LoginPage();
    }

    // Verificar si hay sesión activa
    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(Preferences.Get("SessionId", string.Empty));
    }

    // Cerrar sesión
    public void Logout()
    {
        Preferences.Remove("SessionId");
        Preferences.Remove("UserEmail");
        Preferences.Set("IsLoggedIn", false);
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}