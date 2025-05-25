using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using System.Text.Json;
using System.Text;

namespace SaborSostenibleFrontEnd;

public partial class CodeVerificationPage : ContentPage
{
    private string _email;

    public CodeVerificationPage(string email)
    {
        InitializeComponent();
        _email = email;
    }

    private async void VerifyCode_Clicked(object sender, EventArgs e)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(CodeEntry.Text))
        {
            await DisplayAlert("Error", "Por favor ingrese el c�digo de verificaci�n", "OK");
            return;
        }

        if (CodeEntry.Text.Length != 6)
        {
            await DisplayAlert("Error", "El c�digo debe tener 6 d�gitos", "OK");
            return;
        }

        // Crear el request 
        ReqEmailVerification request = new ReqEmailVerification();
        request.VerificationCode = CodeEntry.Text;
        request.Email = _email;

        using var client = new HttpClient();

        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://34.39.128.125/api/emailVerification", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var verificationResponse = JsonSerializer.Deserialize<ResEmailVerification>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (verificationResponse?.Success == true)
                {
                    await DisplayAlert("�xito", "C�digo verificado correctamente", "OK");
                    await Navigation.PopToRootAsync(); // Volver al logIn
                }
                else
                {
                    // Mostrar errores espec�ficos del servidor
                    var errorMessages = verificationResponse?.Errors?.Select(e => e.Description) ?? new[] { "Error desconocido" };
                    var errorText = string.Join(", ", errorMessages);
                    await DisplayAlert("Error", $"Verificaci�n fallida: {errorText}", "OK");
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"No se pudo verificar el c�digo: {error}", "OK");
            }
        }
        catch (HttpRequestException httpEx)
        {
            await DisplayAlert("Error", $"Error de conexi�n: {httpEx.Message}", "OK");
        }
        catch (JsonException jsonEx)
        {
            await DisplayAlert("Error", $"Error al procesar la respuesta: {jsonEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
        }
    }
}