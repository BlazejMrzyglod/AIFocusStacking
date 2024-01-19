using AIFocusStacking.Services;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AIFocusStacking.Wpf.Pages
{
	public partial class ResultsPage : Page
	{
		protected readonly IRepositoryService<string> _photoRepository;
		public ResultsPage(IRepositoryService<string> photoRepository)
		{
			_photoRepository = photoRepository;
			InitializeComponent();
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
				_ = MainGrid.Children.Add(new Image { Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(photo))) });
			}
		}
	}
}
