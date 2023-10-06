using Microsoft.Win32;
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
         public MainWindow()
        {
            InitializeComponent();          
        }

        //Funkcja umożliwająca wybranie zdjęć
        private void ChooseImagesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            if (fileDialog.ShowDialog() == true)
                SavePhotos(fileDialog.FileNames);
                foreach (var file in fileDialog.FileNames)
                    ImagesWrapPanel.Children.Add(new Image { Source = new BitmapImage(new Uri(file)), Height = 200, Width = 200 });
        }

        //Funkcja odpowiadająca za dodawanie "przeciągniętych" zdjęć
        private void ImagesWrapPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                SavePhotos(files);
                foreach (var file in files)
                    ImagesWrapPanel.Children.Add(new Image { Source = new BitmapImage(new Uri(file)), Height = 200, Width = 200 });
            }
        }

        //Funkcja zapisująca zdjęcia do późniejszej przeróbki
        //TODO: Przenieść do serwisów
        private void SavePhotos(string[] photos)
        {
            int i = 0;
            foreach (var photo in photos) 
            {
                Directory.CreateDirectory("images");
                File.Copy(photo, "images\\photo" + i + ".jpg"); //TODO: Rozszerzenie powinno być zmienne
                i++;
            }
        }

    }
}
