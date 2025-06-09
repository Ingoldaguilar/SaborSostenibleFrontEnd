namespace SaborSostenibleFrontEnd.FoodBankPages;

public partial class FoodBankMainPage : TabbedPage
{
	public FoodBankMainPage()
	{
		InitializeComponent();
        this.CurrentPage = this.Children[0]; // Bolsas Pendientes por defecto
    }
}