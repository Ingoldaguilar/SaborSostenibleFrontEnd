using Microsoft.Maui.Controls;
using SaborSostenibleFrontEnd.Entities;
using System.Collections.ObjectModel;

namespace SaborSostenibleFrontEnd
{
    public partial class MainPage : TabbedPage
    {
        public ObservableCollection<Restaurante> Restaurantes { get; set; }

        public MainPage()
        {
            InitializeComponent();

            Restaurantes = new ObservableCollection<Restaurante>
            {
                new Restaurante
                {
                    idRestaurante = 1,
                    nombreRestaurante = "McDonald's",
                    descripcionRestaurante = "Disfruta de tus clásicos favoritos",
                    imagen = "mcdonaldlogo.png",
                    telefono = "24441234"
                },
                new Restaurante
                {
                    idRestaurante = 2,
                    nombreRestaurante = "Burger King",
                    descripcionRestaurante = "A la parrilla como debe ser",
                    imagen = "burgerkinglogo.png",
                    telefono = "24557890"
                }
            };

            BindingContext = this;
            SaludoLabel.Text = "¡Hola, Usuario! 👋"; // Puedes personalizar esto luego
        }

        private async void OnComprarClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Restaurante restaurante)
            {
                await Navigation.PushAsync(new BuySupriseBagPage(restaurante));
            }
        }
    }


}
