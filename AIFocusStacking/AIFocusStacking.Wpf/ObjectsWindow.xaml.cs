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
using System.Windows.Shapes;

namespace AIFocusStacking.Wpf
{
    /// <summary>
    /// Interaction logic for ObjectsWindow.xaml
    /// </summary>
    public partial class ObjectsWindow : Window
    {
        public ObjectsWindow()
        {
            InitializeComponent();
            var images = Directory.GetFiles("outputImages");
            foreach (var image in images)
            {
                ImagesWrapPanel.Children.Add(new Image { Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory()+ "\\" + image)), Height = 200, Width = 200 });
            }
            
        }
    }
}
