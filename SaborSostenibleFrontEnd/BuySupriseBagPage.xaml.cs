using Microsoft.Maui.Controls;
using SaborSostenibleFrontEnd.Entities;
using System;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd
{
    public partial class BuySupriseBagPage : ContentPage
    {
        private Restaurante _restaurante;
        private string _orderCode;
        private string _customerPhone;

        public BuySupriseBagPage(Restaurante restaurante)
        {
            InitializeComponent();
            _restaurante = restaurante;
            _orderCode = GenerateOrderCode();

            // Mostrar información del restaurante
            RestauranteNombreLabel.Text = _restaurante.nombreRestaurante;
            RestauranteDescripcionLabel.Text = _restaurante.descripcionRestaurante;
            RestauranteImagen.Source = _restaurante.imagen;
            RestauranteTelefonoLabel.Text = _restaurante.telefono;

            // Detalle para SINPE
            SinpeDetalleLabel.Text = $"Pago bolsa sorpresa {_restaurante.nombreRestaurante}-{_orderCode} {_restaurante.telefono}";
        }

        private string GenerateOrderCode()
        {
            Random random = new Random();
            return $"ORD{random.Next(100000, 999999)}";
        }

        private async void OnProcederCompraClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomerPhoneEntry.Text))
            {
                await DisplayAlert("Error", "Por favor ingresa tu número de teléfono", "OK");
                return;
            }

            _customerPhone = CustomerPhoneEntry.Text.Trim();

            // Animación suave de ocultar y mostrar
            await CompraStep.FadeTo(0, 200);
            CompraStep.IsVisible = false;

            InstruccionesStep.Opacity = 0;
            InstruccionesStep.IsVisible = true;
            await InstruccionesStep.FadeTo(1, 200);

            // Actualizar progreso visual
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
            // Deshabilita el botón físico de retroceso
            return true;
        }
    }
}
