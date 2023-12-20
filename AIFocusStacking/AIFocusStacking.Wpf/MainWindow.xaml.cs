using AIFocusStacking.Services;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace AIFocusStacking.Wpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : System.Windows.Window
	{
		protected readonly IPhotoRepositoryService _photoRepository;
		protected readonly IConsoleCommandsService _commandsService;
		protected readonly IFocusStackingService _focusStackingService;
		private bool? alignment;
		private bool? gauss;
		private bool? takeAll;
		private int gaussSize;
		private int laplaceSize;
		private int maskSize;
		private string method;
		public MainWindow(IPhotoRepositoryService photoRepository, IConsoleCommandsService commandsService, IFocusStackingService focusStackingService)
		{
			_photoRepository = photoRepository;
			_commandsService = commandsService;
			InitializeComponent();
			alignment = Alignment.IsChecked;
			gauss = Gauss.IsChecked;
			takeAll = TakeAll.IsChecked;
			gaussSize = Convert.ToInt32(GaussSize.Text);
			laplaceSize = Convert.ToInt32(LaplaceSize.Text);
			maskSize = Convert.ToInt32(MaskSize.Text);
			_focusStackingService = focusStackingService;
		}

		//Funkcja umożliwająca wybranie zdjęć
		private void ChooseImagesButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Multiselect = true;
			if (fileDialog.ShowDialog() == true)
				_photoRepository.CreateMultiple(fileDialog.FileNames);
			foreach (var file in fileDialog.FileNames)
				ImagesWrapPanel.Children.Add(new System.Windows.Controls.Image { Source = new BitmapImage(new Uri(file)), Height = 200, Width = 200 });
		}

		//Funkcja odpowiadająca za dodawanie "przeciągniętych" zdjęć
		private void ImagesWrapPanel_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				_photoRepository.CreateMultiple(files);
				foreach (var file in files)
					ImagesWrapPanel.Children.Add(new System.Windows.Controls.Image { Source = new BitmapImage(new Uri(file)), Height = 200, Width = 200 });
			}
		}

		private void RunFocusStacking_Click(object sender, RoutedEventArgs e)
		{
			IEnumerable<string> photos = _photoRepository.GetAll();
			_commandsService.RunModel();
			_focusStackingService.RunFocusStacking(photos, (bool)alignment, (bool)gauss, laplaceSize, gaussSize, (bool)takeAll, maskSize, method);
			ObjectsWindow objectsWindow = new ObjectsWindow();
			objectsWindow.Show();

		}

		private void Alignment_Click(object sender, RoutedEventArgs e)
		{
			alignment = Alignment.IsChecked;
		}

		private void Gauss_Click(object sender, RoutedEventArgs e)
		{
			gauss = Gauss.IsChecked;
		}

		private void TakeAll_Click(object sender, RoutedEventArgs e)
		{
			takeAll = TakeAll.IsChecked;
		}

		private void GaussSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (GaussSize.Text.Length > 0)
				gaussSize = Convert.ToInt32(GaussSize.Text);
		}

		private void LaplaceSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (LaplaceSize.Text.Length > 0)
				laplaceSize = Convert.ToInt32(LaplaceSize.Text);
		}

		private void MaskSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (MaskSize.Text.Length > 0)
				maskSize = Convert.ToInt32(MaskSize.Text);
		}

		private void SelectMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBoxItem selectedItem = (ComboBoxItem)SelectMethod.SelectedItem;
			method = selectedItem.Content.ToString();
		}
	}

}
