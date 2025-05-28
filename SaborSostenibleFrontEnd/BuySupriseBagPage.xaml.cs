using Microsoft.Maui.Controls;
using SaborSostenibleFrontEnd.Entities;
using System;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd
{
    public partial class BuySupriseBagPage : ContentPage
    {
        private string _orderCode;
        private string _customerPhone;
        private int _currentStep = 1;

        // Propiedad para almacenar el restaurante seleccionado
        public Restaurante RestauranteSeleccionado { get; private set; }

        // Constructor actualizado que recibe un Restaurante
        public BuySupriseBagPage(Restaurante restaurante)
        {
            InitializeComponent();
            RestauranteSeleccionado = restaurante;

            _orderCode = GenerateOrderCode();
            UpdateStepIndicator();

            // Aquí podrías asignar valores a controles si lo deseas más adelante
            // Ejemplo:
            // RestauranteNombreLabel.Text = RestauranteSeleccionado.nombreRestaurante;
        }

        private string GenerateOrderCode()
        {
            Random random = new Random();
            return $"MCDS{random.Next(100000, 999999)}";
        }

        private void UpdateStepIndicator()
        {
            Step1Circle.BackgroundColor = (Color)Application.Current.Resources["Secondary"];
            Step2Circle.BackgroundColor = (Color)Application.Current.Resources["Secondary"];
            Step3Circle.BackgroundColor = (Color)Application.Current.Resources["Secondary"];
            Step4Circle.BackgroundColor = (Color)Application.Current.Resources["Secondary"];

            switch (_currentStep)
            {
                case 1:
                    Step1Circle.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    break;
                case 2:
                    Step2Circle.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    break;
                case 3:
                    Step3Circle.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    break;
                case 4:
                    Step4Circle.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    break;
            }
        }

        private void ShowStep(int step)
        {
            ShoppingStep.IsVisible = false;
            PaymentStep.IsVisible = false;
            ReceiptStep.IsVisible = false;
            WaitingStep.IsVisible = false;

            switch (step)
            {
                case 1:
                    ShoppingStep.IsVisible = true;
                    break;
                case 2:
                    PaymentStep.IsVisible = true;
                    break;
                case 3:
                    ReceiptStep.IsVisible = true;
                    break;
                case 4:
                    WaitingStep.IsVisible = true;
                    break;
            }

            _currentStep = step;
            UpdateStepIndicator();
        }

        private async void OnPurchaseClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CustomerPhoneEntry.Text))
                {
                    await DisplayAlert("Error", "Por favor ingresa tu número de teléfono", "OK");
                    return;
                }

                var phone = CustomerPhoneEntry.Text.Trim();
                if (phone.Length < 8)
                {
                    await DisplayAlert("Error", "Por favor ingresa un número de teléfono válido", "OK");
                    return;
                }

                _customerPhone = phone;

                OrderCodeLabel.Text = _orderCode;
                PaymentDetailLabel.Text = $"Compra bolsa {_orderCode}";
                WhatsAppMessageLabel.Text = $"Comprobante bolsa sorpresa {_orderCode}";
                WaitingOrderCodeLabel.Text = _orderCode;

                ShowStep(2);
            }
            catch (Exception)
            {
                await DisplayAlert("Error", "Ocurrió un error al procesar la compra", "OK");
            }
        }

        private async void OnOpenSinpeClicked(object sender, EventArgs e)
        {
            try
            {
                var uri = "sinpemovil://";
                var canOpen = await Launcher.CanOpenAsync(uri);

                if (canOpen)
                {
                    await Launcher.OpenAsync(uri);
                }
                else
                {
                    await DisplayAlert("SINPE Móvil",
                        "Por favor abre tu aplicación SINPE Móvil manualmente y envía el pago con los datos mostrados.",
                        "OK");
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Información",
                    "Por favor abre tu aplicación SINPE Móvil manualmente y envía el pago con los datos mostrados.",
                    "OK");
            }
        }

        private void OnPaymentSentClicked(object sender, EventArgs e)
        {
            ShowStep(3);
        }

        private async void OnSendWhatsAppClicked(object sender, EventArgs e)
        {
            try
            {
                var phoneNumber = "88888888";
                var message = $"Comprobante bolsa sorpresa {_orderCode}";
                var encodedMessage = Uri.EscapeDataString(message);

                var whatsappUri = $"https://wa.me/506{phoneNumber}?text={encodedMessage}";

                var canOpen = await Launcher.CanOpenAsync(whatsappUri);
                if (canOpen)
                {
                    await Launcher.OpenAsync(whatsappUri);
                }
                else
                {
                    await DisplayAlert("WhatsApp",
                        $"Por favor envía un mensaje a {phoneNumber} con el texto: {message}",
                        "OK");
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Error",
                    "No se pudo abrir WhatsApp. Por favor envía el comprobante manualmente al 8888-8888",
                    "OK");
            }
        }

        private void OnReceiptSentClicked(object sender, EventArgs e)
        {
            ShowStep(4);
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (_currentStep > 1)
                {
                    var result = await DisplayAlert("Confirmar",
                        "¿Estás seguro de que quieres regresar? Se perderá el progreso actual.",
                        "Sí", "No");

                    if (!result)
                        return;
                }

                await Navigation.PopAsync();
            }
            catch (Exception)
            {
                await Navigation.PopAsync();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BackButton != null)
            {
                BackButton.Clicked -= OnBackButtonClicked;
                BackButton.Clicked += OnBackButtonClicked;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        public async Task SimulatePaymentConfirmation()
        {
            await Task.Delay(5000);

            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("¡Pago Confirmado!",
                    $"Tu bolsa sorpresa {_orderCode} está lista para recoger. Te hemos enviado los detalles por WhatsApp.",
                    "OK");
            });
        }

        public void ResetPage()
        {
            _currentStep = 1;
            CustomerPhoneEntry.Text = string.Empty;
            _customerPhone = string.Empty;
            _orderCode = GenerateOrderCode();
            ShowStep(1);
        }
    }

}
