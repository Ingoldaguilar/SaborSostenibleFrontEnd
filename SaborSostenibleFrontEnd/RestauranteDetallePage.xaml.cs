using SaborSostenibleFrontEnd.Entities;

namespace SaborSostenibleFrontEnd;

public partial class RestauranteDetallePage : ContentPage
{
    public RestauranteDetallePage(Restaurante restaurante)
    {
        InitializeComponent();

        ImagenRestaurante.Source = restaurante.imagen;
        NombreRestauranteLabel.Text = restaurante.nombreRestaurante;
        UbicacionRestauranteLabel.Text = $"Ubicaci�n: {restaurante.descripcionRestaurante}";
        DescripcionRestauranteLabel.Text = $"Descripci�n: {restaurante.descripcionRestaurante}";
    }
}
