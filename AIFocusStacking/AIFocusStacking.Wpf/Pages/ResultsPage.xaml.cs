using AIFocusStacking.Models;
using AIFocusStacking.Services;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
				MainGrid.Children.Add(new Image { Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(photo))) });
			}
		}
	}
}
