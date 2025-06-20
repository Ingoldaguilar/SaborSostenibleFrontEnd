﻿using SaborSostenibleFrontEnd.Response;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
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

    // POST multipart/form-data con los campos de 'data' y un solo archivo.
    public async Task<TResponse?> PostMultipartAsync<TRequest, TResponse>(
            string endpoint,
            TRequest data,
            FileResult file,
            string fileParamName)
            where TResponse : class
    {
        try
        {
            SetBearerToken();

            using var content = new MultipartFormDataContent();

            // Añadimos cada propiedad de data como StringContent
            var props = typeof(TRequest)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetValue(data) != null);

            foreach (var prop in props)
            {
                var raw = prop.GetValue(data)!;
                string value = raw is IFormattable formattable
                    ? formattable.ToString(null, CultureInfo.InvariantCulture)
                    : raw.ToString()!;

                content.Add(new StringContent(value, System.Text.Encoding.UTF8), prop.Name);
            }

            // Añadimos el archivo
            using var stream = await file.OpenReadAsync();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
            content.Add(fileContent, fileParamName, file.FileName);

            // Ejecutamos el POST
            var resp = await _httpClient.PostAsync(endpoint, content);

            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorized();
                return null;
            }
            else
            {
                throw new HttpRequestException($"Error en API multipart ({resp.StatusCode})");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error en POST multipart '{endpoint}': {ex.Message}", ex);
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
        Preferences.Remove("UserRole");
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

    public async Task Logout()
    {
        try
        {
            ResBase resBase = new();
            var sessionId = Preferences.Get("SessionId", null);

            if (!string.IsNullOrEmpty(sessionId))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionId);
            }

            var response = await _httpClient.PostAsync("logOut", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Deserializar la respuesta aunque sea 200
            resBase = JsonSerializer.Deserialize<ResBase>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al cerrar sesión en el servidor: {errorContent}");
                // (Opcional) Mostrar mensaje al usuario
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en Logout: {ex.Message}");
            // (Opcional) Mostrar mensaje de error al usuario
        }

        // Limpieza local sin importar si el logout fue exitoso
        Preferences.Remove("SessionId");
        Preferences.Remove("UserEmail");
        Preferences.Remove("UserRole");
        Preferences.Set("IsLoggedIn", false);
        _httpClient.DefaultRequestHeaders.Authorization = null;

        // Mostrar mensaje de confirmación
        await Application.Current.MainPage.DisplayAlert("Sesión cerrada",
            "Su sesión se ha cerrado exitosamente.", "OK");

        // Redirigir al login
        Application.Current.MainPage = new NavigationPage(new LoginPage());
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}