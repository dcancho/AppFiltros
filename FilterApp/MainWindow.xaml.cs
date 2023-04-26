using System;
using System.Collections.Generic;
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

namespace FilterApp
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Call a open file dialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg|All Files (*.*)|*.*";
            //Show the dialog
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                //Open document
                string filename = dlg.FileName;
                //Create a new image
                Image img = new Image();
                //Create a new bitmap image
                BitmapImage bitmap = new BitmapImage();
                //Set the image source
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filename);
                bitmap.EndInit();
                img.Source = bitmap;
                //Set as ImageContainer's source
                ActiveImage.Source = img.Source;
            }
        }
    }
}
