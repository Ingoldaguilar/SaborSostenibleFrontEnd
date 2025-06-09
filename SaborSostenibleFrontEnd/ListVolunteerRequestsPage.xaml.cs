using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;                     // <-- para Colors.Gray
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd
{
    public partial class ListVolunteerRequestsPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();

        public ListVolunteerRequestsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadRequestsAsync();
        }

        private async Task LoadRequestsAsync()
        {
            try
            {
                RequestsContainer.Children.Clear();

                var resp = await _api.GetAsync<ResPendingVolunteerRequests>("allPendingVolunteerRequests/get");
                if (resp == null)
                {
                    throw new Exception("La respuesta es nula.");
                }

                if (!resp.Success || resp.Requests?.Any() != true)
                {
                    RequestsContainer.Children.Add(new Label
                    {
                        Text = "Actualmente no hay solicitudes para voluntarios.",
                        FontSize = 16,
                        TextColor = Colors.Gray,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.CenterAndExpand
                    });
                    return;
                }

                foreach (var req in resp.Requests)
                {
                    var frame = new Frame
                    {
                        CornerRadius = 8,
                        Padding = 12,
                        Margin = new Thickness(0, 0, 0, 10),
                        BackgroundColor = Colors.White,
                        HasShadow = true
                    };

                    var nameLabel = new Label
                    {
                        Text = req.FullName,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16,
                        TextColor = Colors.Black
                    };

                    var dateLabel = new Label
                    {
                        Text = req.RequestDate.ToLocalTime().ToString("dd/MM/yyyy"),
                        FontSize = 14,
                        TextColor = Colors.Gray
                    };

                    var deny = new Button
                    {
                        Visual = VisualMarker.Default,
                        Text = "Denegar",
                        BackgroundColor = Color.FromArgb("#E53935"),
                        TextColor = Colors.White,
                        CornerRadius = 15,
                        FontSize = 14,
                        HeightRequest = 32,
                        Padding = new Thickness(20, 0),
                        CommandParameter = req.RequestId
                    };
                    deny.Clicked += (s, e) => _ = ProcessRequestAsync((int)deny.CommandParameter, false);

                    var approve = new Button
                    {
                        Visual = VisualMarker.Default,
                        Text = "Aprobar",
                        BackgroundColor = Color.FromArgb("#3E7B31"),
                        TextColor = Colors.White,
                        CornerRadius = 15,
                        FontSize = 14,
                        HeightRequest = 32,
                        Padding = new Thickness(20, 0),
                        CommandParameter = req.RequestId
                    };
                    approve.Clicked += (s, e) => _ = ProcessRequestAsync((int)approve.CommandParameter, true);

                    var btnLayout = new HorizontalStackLayout
                    {
                        Spacing = 10,
                        HorizontalOptions = LayoutOptions.End,
                        Children = { deny, approve }
                    };

                    frame.Content = new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children = { nameLabel, dateLabel, btnLayout }
                    };

                    RequestsContainer.Children.Add(frame);
                }
            }
            catch (Exception ex)
            {
                // si falla, mostramos un mensaje para diagnosticar
                await DisplayAlert("Error cargando solicitudes", ex.Message, "OK");
            }
        }

        private async Task ProcessRequestAsync(int requestId, bool status)
        {
            var loader = new LoadingPage();
            await Navigation.PushModalAsync(loader);

            var req = new ReqProcessVolunteerRequest
            {
                RequestId = requestId,
                Status = status
            };

            var res = await _api.PostAsync<ReqProcessVolunteerRequest, ResBase>(
                "processVolunteerRequest/update", req);

            await Navigation.PopModalAsync();

            if (res?.Success == true)
            {
                await DisplayAlert("Éxito",
                    status ? "Solicitud aprobada" : "Solicitud denegada", "OK");
                await LoadRequestsAsync();
            }
            else
            {
                var errs = res?.Errors?.Select(x => x.Description) ?? new[] { "Error desconocido" };
                await DisplayAlert("Error", string.Join("\n", errs), "OK");
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}
