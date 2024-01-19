using AIFocusStacking.Services;
using AIFocusStacking.Wpf.Pages;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
			MainFrame.Navigate(homePage);
		}
	}
}
