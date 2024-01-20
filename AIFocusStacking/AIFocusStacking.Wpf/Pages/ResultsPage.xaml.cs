using Microsoft.Win32;
using OpenCvSharp;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AIFocusStacking.Wpf.Pages
{
	public partial class ResultsPage : Page
	{
		protected readonly HomePage _homePage;
		protected string[] photos;

		public ResultsPage(HomePage homePage)
		{
			_homePage = homePage;
			InitializeComponent();
			Loaded += ResultsPage_Loaded;
			SizeChanged += ResultsPage_SizeChanged;
			photos = Directory.GetFiles("outputImages");
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
					image.MaxHeight = maxHeight - 100 > 0 ? maxHeight - 100 : 10;
					image.MaxWidth = maxWidth - 300 > 0 ? maxWidth - 300 : 10;
				}
			}

			// Repeat similar logic for LaplacePanel and DetectionPanel
		}
		public void GetResults()
		{
			foreach (string photo in photos)
			{
				BitmapImage bitmap = new();
				bitmap.BeginInit();
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.UriSource = new(Path.GetFullPath(photo));
				bitmap.EndInit();

				_ = photo.Contains("result")
					? ResultPanel.Children.Add(new Image { Source = bitmap, MaxHeight = ScrollViewer.ActualHeight - 100 > 0 ? ScrollViewer.ActualHeight - 100 : 10, MaxWidth = ScrollViewer.ActualWidth - 300 > 0 ? ScrollViewer.ActualWidth - 300 : 10 })
					: photo.Contains("laplace")
						? LaplacePanel.Children.Add(new CustomResultImage(bitmap))
						: DetectionPanel.Children.Add(new CustomResultImage(bitmap));
			}
		}

		private void BackArrow_Click(object sender, RoutedEventArgs e)
		{
			ResultPanel.Children.Clear();
			LaplacePanel.Children.Clear();
			DetectionPanel.Children.Clear();
			Directory.Delete("outputImages", true);
			_ = NavigationService.Navigate(_homePage);
		}

		private void SaveImage_Click(object sender, RoutedEventArgs e)
		{
			string result = System.IO.Path.GetFullPath(photos.Where(r => r.Contains("result")).First());
			SaveFileDialog fileDialog = new()
			{
				FileName = result.Split("\\").Last()
			};

			if (fileDialog.ShowDialog() == true)
			{
				File.Copy(result, fileDialog.FileName);
			}
		}
	}
	public class CustomResultImage : UserControl
	{
		protected Image _image;
		protected Button _button;
		protected string _name;

		public CustomResultImage(BitmapImage bitmap)
		{
			_image = new Image
			{
				Source = bitmap,
				Margin = new Thickness(5)
			};
			_button = new Button
			{
				Background = Brushes.Transparent,
				BorderThickness = new Thickness(0),
				Cursor = Cursors.Hand
			};
			_button.Click += Button_Click;

			_name = bitmap.UriSource.OriginalString;

			InitializeComponents();
		}

		private void InitializeComponents()
		{
			Grid grid = new();
			_ = grid.Children.Add(_image);
			_ = grid.Children.Add(_button);

			Content = grid;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Mat image = Cv2.ImRead(_name);
			Cv2.NamedWindow(_name.Split("\\").Last(), WindowFlags.KeepRatio);
			Cv2.ImShow(_name.Split("\\").Last(), image);
		}
	}
}
