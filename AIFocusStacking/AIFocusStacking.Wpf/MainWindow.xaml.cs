﻿using AIFocusStacking.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AIFocusStacking.Wpf
{
	//Główne okno programu
	public partial class MainWindow : Window
	{
		//Repozytorium zdjęć
		protected readonly IPhotoRepositoryService _photoRepository;

		//Serwis wykonujący focus stacking
		protected readonly IFocusStackingService _focusStackingService;

		//Czy wyrównywać zdjęcia?
		private bool? alignment;

		//Czy używać filtru gaussa?
		private bool? gauss;

		//Czy traktować piksele w masce jako jedność?
		private bool? takeAll;

		//Wielkość maski dla filtru Gaussa
		private int gaussSize;

		//Wielkość maski dla filtru Laplace'a
		private int laplaceSize;

		//Wielkość maski dla sprawdzania instnsywności
		private int maskSize;

		//Której metody focus stackingu użyć 
		//1 - bez AI
		//2 - Segmentacjia instancji
		//3 - Panoptyczna segmentacja
		private string? method;

		//Wartość pewności powyżej której wykryty obiekt jest rejestrowany
		private string confidence;

		public MainWindow(IPhotoRepositoryService photoRepository, IFocusStackingService focusStackingService)
		{
			_photoRepository = photoRepository;
			InitializeComponent();
			alignment = Alignment.IsChecked;
			gauss = Gauss.IsChecked;
			takeAll = TakeAll.IsChecked;
			gaussSize = Convert.ToInt32(GaussSize.Text);
			laplaceSize = Convert.ToInt32(LaplaceSize.Text);
			maskSize = Convert.ToInt32(MaskSize.Text);
			_focusStackingService = focusStackingService;
			confidence = Confidence.Text;
		}

		//Funkcja umożliwająca wybranie zdjęć
		private void ChooseImagesButton_Click(object sender, RoutedEventArgs e)
		{
			//Otwórz dialog wyboru plików z opcją wielokrotnego wyboru
			OpenFileDialog fileDialog = new()
			{
				Multiselect = true
			};

			//Stwórz pliki w repozytorium zdjęć
			if (fileDialog.ShowDialog() == true)
			{
				_ = _photoRepository.CreateMultiple(fileDialog.FileNames);
			}

			//Dodaj zdjęcia do obszaru wyświetlającego zdjęcia
			foreach (string? file in fileDialog.FileNames)
			{
				_ = ImagesWrapPanel.Children.Add(new Image { Source = new BitmapImage(new Uri(file)), Height = 200, Width = 200 });
			}
		}

		//Funkcja odpowiadająca za dodawanie "przeciągniętych" zdjęć
		private void ImagesWrapPanel_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				//Pobierz przeciągnięte pliki
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				//Stwórz pliki w repozytorium zdjęć
				_ = _photoRepository.CreateMultiple(files);

				//Dodaj zdjęcia do obszaru wyświetlającego zdjęcia
				foreach (string file in files)
				{
					_ = ImagesWrapPanel.Children.Add(new Image { Source = new BitmapImage(new Uri(file)), Height = 200, Width = 200 });
				}
			}
		}

		//Funkcja uruchamiająca focus stacking
		private void RunFocusStacking_Click(object sender, RoutedEventArgs e)
		{
			//Pobierz zdjęcia z repozytorium
			IEnumerable<string> photos = _photoRepository.GetAll();

			//Uruchom focus stacking
			_ = _focusStackingService.RunFocusStacking(photos, (bool)alignment!, (bool)gauss!, laplaceSize, gaussSize, (bool)takeAll!, maskSize, method!, confidence);
			/*ObjectsWindow objectsWindow = new ObjectsWindow();
			objectsWindow.Show();*/

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
			{
				gaussSize = Convert.ToInt32(GaussSize.Text);
			}
		}

		private void LaplaceSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (LaplaceSize.Text.Length > 0)
			{
				laplaceSize = Convert.ToInt32(LaplaceSize.Text);
			}
		}

		private void MaskSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (MaskSize.Text.Length > 0)
			{
				maskSize = Convert.ToInt32(MaskSize.Text);
			}
		}

		private void SelectMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBoxItem selectedItem = (ComboBoxItem)SelectMethod.SelectedItem;
			method = selectedItem.Content.ToString();
		}

		private void Confidence_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (Confidence.Text.Length > 0)
			{
				confidence = Confidence.Text;
			}
		}
	}

}
