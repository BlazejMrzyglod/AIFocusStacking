using AIFocusStacking.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace AIFocusStacking.Wpf.Pages
{
	/// <summary>
	/// Interaction logic for HomePage.xaml
	/// </summary>
	public partial class HomePage : Page
	{
		//Repozytorium zdjęć
		protected readonly IRepositoryService<string> _photoRepository;

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

		public HomePage(IRepositoryService<string> photoRepository, IFocusStackingService focusStackingService)
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
				_ = _photoRepository.AddMultiple(fileDialog.FileNames);
			}

			//Dodaj zdjęcia do obszaru wyświetlającego zdjęcia
			foreach (string? file in fileDialog.FileNames)
			{
				_ = ImagesWrapPanel.Children.Add(new CustomImage(new Uri(file), ImagesWrapPanel, _photoRepository));
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
				_ = _photoRepository.AddMultiple(files);

				//Dodaj zdjęcia do obszaru wyświetlającego zdjęcia
				foreach (string file in files)
				{
					_ = ImagesWrapPanel.Children.Add(new CustomImage(new Uri(file), ImagesWrapPanel, _photoRepository));
				}
			}
		}

		//Funkcja uruchamiająca focus stacking
		private void RunFocusStacking_Click(object sender, RoutedEventArgs e)
		{
			//Uruchom ikonę ładowania
			LoadingSpinner.Visibility = Visibility.Visible;

			//Pobierz zdjęcia z repozytorium
			IEnumerable<string> photos = _photoRepository.GetAll();

			//Uruchom focus stacking
			ServiceResult result = _focusStackingService.RunFocusStacking(photos, (bool)alignment!, (bool)gauss!, laplaceSize, gaussSize, (bool)takeAll!, maskSize, method!, confidence);
			if (result.Result == ServiceResultStatus.Error)
			{
				MessageBoxButton button = MessageBoxButton.OK;
				MessageBoxImage icon = MessageBoxImage.Error;

				string messages = "";
				foreach (string message in result.Messages)
				{
					messages += message;
					messages += "\n";
				}

				_ = MessageBox.Show(messages, "Błąd", button, icon);
			}
			else if (result.Result == ServiceResultStatus.Succes)
			{
				ResultsPage _resultPage = new(this);
				_ = NavigationService.Navigate(_resultPage);
			}

			//Wyłącz ikonę ładowania
			LoadingSpinner.Visibility = Visibility.Hidden;

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
			method = selectedItem.Content.ToString() == "Bez SI" ? "1" : selectedItem.Content.ToString() == "Segmentacja instancji" ? "2" : "3";
		}

		private void Confidence_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (Confidence.Text.Length > 0)
			{
				confidence = Confidence.Text;
			}
		}

		private void DeletePhotos_Click(object sender, RoutedEventArgs e)
		{
			_ = _photoRepository.DeleteAll();
			ImagesWrapPanel.Children.Clear();
		}
	}


	public class CustomImage : UserControl
	{
		protected Image _image;
		protected Button _button;
		protected WrapPanel _wrapPanel;
		protected IRepositoryService<string> _photoRepository;
		protected string _name;

		public CustomImage(Uri imageUri, WrapPanel wrapPanel, IRepositoryService<string> photoRepository)
		{
			_image = new Image
			{
				Source = new BitmapImage(imageUri),
				Height = 200,
				Width = 200,
				Margin = new Thickness(5)
			};
			_button = new Button
			{
				Content = "Usuń",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Visibility = Visibility.Hidden,

			};
			_button.Click += Button_Click;

			_wrapPanel = wrapPanel;
			_photoRepository = photoRepository;
			_name = imageUri.OriginalString.Split("\\").Last();

			InitializeComponents();
			MouseEnter += CustomImage_MouseEnter;
			MouseLeave += CustomImage_MouseLeave;
		}

		private void InitializeComponents()
		{
			Grid grid = new();
			_ = grid.Children.Add(_image);
			_ = grid.Children.Add(_button);

			Content = grid;
		}

		private void CustomImage_MouseEnter(object sender, MouseEventArgs e)
		{
			_button.Visibility = Visibility.Visible;
		}

		private void CustomImage_MouseLeave(object sender, MouseEventArgs e)
		{
			_button.Visibility = Visibility.Hidden;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_wrapPanel.Children.Remove(this);
			_ = _photoRepository.Delete(_name);
		}
	}
}
