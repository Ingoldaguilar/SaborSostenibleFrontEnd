namespace SaborSostenibleFrontEnd.BusinessPages;

public partial class BusinessMainPage : TabbedPage
{
	public BusinessMainPage()
	{
		InitializeComponent();
        this.CurrentPage = this.Children[0]; // Nueva bolsa por defecto
    }
}