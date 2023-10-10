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
        private void run_cmd(object sender, RoutedEventArgs e)
        {
            IEnumerable<string> photos = _photoRepository.GetAll();
            ProcessStartInfo start = new ProcessStartInfo();
            string script = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\demo\\demo.py";
            string configFile = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\projects\\PointRend\\configs\\InstanceSegmentation\\pointrend_rcnn_X_101_32x8d_FPN_3x_coco.yaml";
            string outputDirectory = "outputImages";
            Directory.CreateDirectory(outputDirectory);
            string options = "MODEL.DEVICE cpu";
            string weights = "..\\..\\..\\..\\..\\..\\Detectron2\\detectron2\\demo\\model_final_ba17b9.pkl";
            start.FileName = "CMD.exe";
            start.Arguments = $"/C python {script} --config-file {configFile} --input {string.Join(" ", photos)} --output {outputDirectory} --opts {options} MODEL.WEIGHTS {weights}";
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
            _photoRepository.DeleteMultiple(photos.ToArray());
        }

    }
}
