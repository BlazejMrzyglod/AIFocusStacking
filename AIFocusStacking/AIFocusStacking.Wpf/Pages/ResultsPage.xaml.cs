using AIFocusStacking.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AIFocusStacking.Wpf.Pages
{
	public partial class ResultsPage : Page
	{
		protected readonly HomePage _homePage;

		public ResultsPage(HomePage homePage)
		{
			_homePage = homePage;
			InitializeComponent();
			Loaded += ResultsPage_Loaded;
			SizeChanged += ResultsPage_SizeChanged;
		}
		private void ResultsPage_Loaded(object sender, RoutedEventArgs e)
		{
			GetResults(); // Load results initially
		}

		private void ResultsPage_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateImageSizes(); // Update image sizes when the window size changes
		}
		private void UpdateImageSizes()
		{
			double maxHeight = ScrollViewer.ActualHeight;
			double maxWidth = ScrollViewer.ActualWidth;

			foreach (UIElement element in ResultPanel.Children)
			{
				if (element is Image image)
				{
					image.MaxHeight = maxHeight - 100;
					image.MaxWidth = maxWidth - 300;
				}
			}

			// Repeat similar logic for LaplacePanel and DetectionPanel
		}
		public void GetResults()
		{
			string[] photos = Directory.GetFiles("outputImages");
			/*if (photos.Where(r => r.Contains("result")).Any())
			{
				MainGrid.Children.Add(new Image { Source = new BitmapImage(new Uri(photos.Where(r => r.Contains("result")).FirstOrDefault()!)) });
			}*/
			foreach (string photo in photos)
			{
				if (photo.Contains("result"))
				{
					_ = ResultPanel.Children.Add(new Image { Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(photo))), MaxHeight=ScrollViewer.ActualHeight - 100, MaxWidth=ScrollViewer.ActualWidth - 300});
				}

				else if(photo.Contains("laplace"))
				{
					_ = LaplacePanel.Children.Add(new Image { Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(photo))), Margin = new Thickness(5) });
				}

				else 
				{
					_ = DetectionPanel.Children.Add(new Image { Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(photo))), Margin = new Thickness(5) });
				}
			}
		}

		private void BackArrow_Click(object sender, RoutedEventArgs e)
		{
			ResultPanel.Children.Clear();
			LaplacePanel.Children.Clear();
			DetectionPanel.Children.Clear();
			NavigationService.Navigate(_homePage);
		}
	}
}
