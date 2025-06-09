using SaborSostenibleFrontEnd.Entities;

namespace SaborSostenibleFrontEnd;

public partial class RestauranteDetallePage : ContentPage
{
    public RestauranteDetallePage(Restaurante restaurante)
    {
        InitializeComponent();

        ImagenRestaurante.Source = restaurante.imagen;
        NombreRestauranteLabel.Text = restaurante.nombreRestaurante;
        UbicacionRestauranteLabel.Text = $"Ubicación: {restaurante.descripcionRestaurante}";
        DescripcionRestauranteLabel.Text = $"Descripción: {restaurante.descripcionRestaurante}";
    }
}
