using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd
{
    public partial class BusinessPendingSaleDetailsPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private readonly int _orderId;

        public BusinessPendingSaleDetailsPage(int orderId)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _orderId = orderId;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadDetailsAsync();
        }

        private async Task LoadDetailsAsync()
        {
            var req = new ReqBusinessPendingSaleDetails { OrderId = _orderId };
            var res = await _api.PostAsync<ReqBusinessPendingSaleDetails, ResBusinessPendingSaleDetails>(
                "businessPendingSaleDetails/post", req);

            if (res == null || !res.Success || res.Detail == null)
            {
                var msg = res == null
                    ? "Sin respuesta del servidor."
                    : string.Join("\n", res.Errors.Select(e => e.Description));
                await DisplayAlert("Error", msg, "OK");
                return;
            }

            OrderCodeLabel.Text = res.Detail.OrderCode;
            StateLabel.Text = $"Estado actual: {res.Detail.StateText}";
            TotalLabel.Text = $"Total: {res.Detail.TotalAmount} colones";
        }

        private void OnViewBagsClicked(object sender, EventArgs e)
            => _ = Navigation.PushAsync(new OrderDetailsPage(_orderId));

        private async void OnConfirmPaymentClicked(object sender, EventArgs e)
        {
            await UpdatePaymentStatusAsync(6); // 6 = Completado
        }

        private async void OnDenyPaymentClicked(object sender, EventArgs e)
        {
            await UpdatePaymentStatusAsync(10); //10 = Denegado
        }

        private async Task UpdatePaymentStatusAsync(int stateCode)
        {
            var req = new ReqPaymentStatusUpdate
            {
                OrderId = _orderId,
                StateCode = stateCode
            };
            var res = await _api.PostAsync<ReqPaymentStatusUpdate, ResBase>(
                "paymentStatus/update", req);

            if (res?.Success == true)
            {
                await DisplayAlert("Éxito",
                    stateCode == 6 ? "Pago confirmado." : "Pago denegado.",
                    "OK");
                await Navigation.PopToRootAsync();
            }
            else
            {
                var errs = res?.Errors?.Select(x => x.Description)
                           ?? new[] { "Error desconocido" };
                await DisplayAlert("Error", string.Join("\n", errs), "OK");
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}
