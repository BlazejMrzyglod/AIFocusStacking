using AIFocusStacking.Wpf.Pages;
using System.Windows;

namespace AIFocusStacking.Wpf
{
	//Główne okno programu
	public partial class MainWindow : Window
	{
		//Główna strona programu
		protected readonly HomePage homePage;
		public MainWindow(HomePage homePage)
		{
			InitializeComponent();
			_ = MainFrame.Navigate(homePage);
		}
	}
}
