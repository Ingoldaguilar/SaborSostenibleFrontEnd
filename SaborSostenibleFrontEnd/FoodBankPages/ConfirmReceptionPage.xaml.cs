using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SaborSostenibleFrontEnd.Entities;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.FoodBankPages
{
    public partial class ConfirmReceptionPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private readonly PendingBag _bag;

        public ConfirmReceptionPage(PendingBag bag)
        {
            InitializeComponent();
            _bag = bag;

            // Poblamos UI
            LogoImage.Source = ImageSource.FromUri(new Uri($"http://34.39.128.125/{bag.LogoUrl}"));
            BusinessNameLabel.Text = bag.BusinessName;
            DateLabel.Text = bag.DonationDate.ToLocalTime().ToString("dd/MM/yyyy");
            DescriptionLabel.Text = bag.BagDescription;
            StateLabel.Text = bag.State;
        }

        private async void OnConfirmClicked(object sender, EventArgs e)
        {
            var loader = new LoadingPage();
            await Navigation.PushModalAsync(loader);

            var req = new ReqConfirmBagReceptionByFoodBank
            {
                BagId = _bag.BagId
            };

            var res = await _api.PostAsync<ReqConfirmBagReceptionByFoodBank, ResBase>(
                "confirmBagReceptionByFoodBank/post", req);

            await Navigation.PopModalAsync();

            if (res?.Success == true)
            {
                await DisplayAlert("Éxito", "Recepción confirmada.", "OK");
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
