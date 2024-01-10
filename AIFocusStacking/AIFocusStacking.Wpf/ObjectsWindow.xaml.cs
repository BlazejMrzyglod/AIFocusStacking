using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AIFocusStacking.Wpf
{
	/// <summary>
	/// Interaction logic for ObjectsWindow.xaml
	/// </summary>
	public partial class ObjectsWindow : Window
	{
		public ObjectsWindow()
		{
			InitializeComponent();
			string[] images = Directory.GetFiles("outputImages");
			foreach (string image in images)
			{
				_ = ImagesWrapPanel.Children.Add(new Image { Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\" + image)), Height = 200, Width = 200 });
			}

		}
	}
}
