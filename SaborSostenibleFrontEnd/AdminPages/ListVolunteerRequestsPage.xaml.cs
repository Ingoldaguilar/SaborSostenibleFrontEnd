using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SaborSostenibleFrontEnd.Request;
using SaborSostenibleFrontEnd.Response;
using SaborSostenibleFrontEnd.Security;

namespace SaborSostenibleFrontEnd.AdminPages
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
                    throw new Exception("La respuesta es nula.");

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
                    // tarjeta
                    var frame = new Frame
                    {
                        CornerRadius = 8,
                        Padding = 12,
                        Margin = new Thickness(0, 0, 0, 10),
                        BackgroundColor = Colors.White,
                        HasShadow = true,
                        BorderColor = Color.FromRgba(0, 0, 0, 0.02),
                        HorizontalOptions = LayoutOptions.Fill  // asegúrate de que el frame llene
                    };

                    // Nombre
                    var nameLabel = new Label
                    {
                        Text = req.FullName,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16,
                        TextColor = Colors.Black
                    };

                    // Fecha
                    var dateLayout = new HorizontalStackLayout
                    {
                        Spacing = 4,
                        VerticalOptions = LayoutOptions.Center
                    };
                    dateLayout.Children.Add(new Label
                    {
                        Text = "\uf133",           // FontAwesome calendar
                        FontFamily = "FontAwesome",
                        FontSize = 10,
                        TextColor = Colors.Gray,
                        VerticalOptions = LayoutOptions.Center
                    });
                    dateLayout.Children.Add(new Label
                    {
                        Text = req.RequestDate.ToLocalTime().ToString("dd/MM/yyyy"),
                        FontSize = 12,
                        TextColor = Colors.Gray,
                        VerticalOptions = LayoutOptions.Center
                    });

                    // Botones con Grid para ancho idéntico
                    var approve = new Button
                    {
                        Text = "Aprobar",
                        BackgroundColor = Color.FromArgb("#789262"),
                        TextColor = Colors.White,
                        CornerRadius = 6,
                        FontSize = 12,
                        HeightRequest = 36,
                        FontAttributes = FontAttributes.Bold,
                        CommandParameter = req.RequestId
                    };
                    approve.Clicked += (s, e) => _ = ProcessRequestAsync((int)approve.CommandParameter, true);

                    var deny = new Button
                    {
                        Text = "Denegar",
                        BackgroundColor = Colors.White,
                        BorderColor = Colors.Gray,
                        BorderWidth = 1,
                        TextColor = Colors.Gray,
                        CornerRadius = 6,
                        FontSize = 12,
                        HeightRequest = 36,
                        FontAttributes = FontAttributes.Bold,
                        CommandParameter = req.RequestId
                    };
                    deny.Clicked += (s, e) => _ = ProcessRequestAsync((int)deny.CommandParameter, false);

                    var btnGrid = new Grid
                    {
                        ColumnSpacing = 10,
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Star }
                        },
                        HorizontalOptions = LayoutOptions.Fill
                    };
                    btnGrid.Add(approve, 0, 0);
                    btnGrid.Add(deny, 1, 0);

                    frame.Content = new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children = { nameLabel, dateLayout, btnGrid }
                    };

                    RequestsContainer.Children.Add(frame);
                }
            }
            catch (Exception ex)
            {
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
                await DisplayAlert(
                    "Éxito",
                    status ? "Solicitud aprobada" : "Solicitud denegada",
                    "OK");
                await LoadRequestsAsync();
            }
            else
            {
                var errs = res?.Errors?
                              .Select(x => x.Description)
                              ?? new[] { "Error desconocido" };
                await DisplayAlert("Error", string.Join("\n", errs), "OK");
            }
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
            => _ = Navigation.PopAsync();
    }
}
