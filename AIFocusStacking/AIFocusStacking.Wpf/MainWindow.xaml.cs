using AIFocusStacking.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AIFocusStacking.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected IPhotoRepositoryService _photoRepository;
        protected IConsoleCommandsService _commandsService;
         public MainWindow(IPhotoRepositoryService photoRepository, IConsoleCommandsService commandsService)
        {
            _photoRepository = photoRepository;
            _commandsService = commandsService;
            InitializeComponent();          
        }

        //Funkcja umożliwająca wybranie zdjęć
        private void ChooseImagesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            if (fileDialog.ShowDialog() == true)
                _photoRepository.CreateMultiple(fileDialog.FileNames);
                foreach (var file in fileDialog.FileNames)
                    ImagesWrapPanel.Children.Add(new Image { Source = new BitmapImage(new Uri(file)), Height = 200, Width = 200 });
        }

        //Funkcja odpowiadająca za dodawanie "przeciągniętych" zdjęć
        private void ImagesWrapPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                _photoRepository.CreateMultiple(files);
                foreach (var file in files)
                    ImagesWrapPanel.Children.Add(new Image { Source = new BitmapImage(new Uri(file)), Height = 200, Width = 200 });
            }
        }

        //Funkcja uruchamiająca detectrona w konsoli
        //TODO: Przenieść do serwisów
        private void RunModelButton_Click(object sender, RoutedEventArgs e)
        {
            _commandsService.RunModel();
            ObjectsWindow objectsWindow = new ObjectsWindow();
            objectsWindow.Show();
        }

    }
}
