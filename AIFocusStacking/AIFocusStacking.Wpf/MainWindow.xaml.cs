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
        protected IPhotoRepository _photoRepository;
         public MainWindow(IPhotoRepository photoRepository)
        {
            _photoRepository = photoRepository;
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
            _photoRepository.CreateMultiple(photos);
        }

        //Funkcja uruchamiająca detectrona w konsoli
        //TODO: Przenieść do serwisów
        private void run_cmd(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "CMD.exe";
            start.Arguments = "/C python C:\\Users\\blaze\\Pulpit\\Studia\\Inżynierka\\Detectron2\\detectron2\\demo\\demo.py --config-file C:\\Users\\blaze\\Pulpit\\Studia\\Inżynierka\\Detectron2\\detectron2\\projects\\PointRend\\configs\\InstanceSegmentation\\pointrend_rcnn_X_101_32x8d_FPN_3x_coco.yaml  --input C:\\Users\\blaze\\Pulpit\\Studia\\Inżynierka\\Detectron2\\detectron2\\demo\\pokojTest3.jpg --output C:\\Users\\blaze\\Pulpit\\Studia\\Inżynierka\\Detectron2\\detectron2\\demo\\Output --opts MODEL.DEVICE cpu MODEL.WEIGHTS C:\\Users\\blaze\\Pulpit\\Studia\\Inżynierka\\Detectron2\\detectron2\\demo\\model_final_ba17b9.pkl";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }

    }
}
