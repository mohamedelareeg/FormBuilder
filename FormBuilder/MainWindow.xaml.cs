using FormBuilder.Models;
using FormBuilder.Windows;
using Microsoft.Win32;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FormBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        private ImageItem selectedImage;
        private int selectedImageIndex;
        private int ImagesCount;
        private Guid selectedGuid;
        public MainWindow(ImageItem _selectedImage, int _selectedImageIndex, Guid _selectedGuid, int imagesCount)
        {
            this.selectedImage = _selectedImage;
            this.selectedImageIndex = _selectedImageIndex;
            this.selectedGuid = _selectedGuid;

            InitializeComponent();
            ScrollViewer.Content = new Image { Source = selectedImage.Image };
            ImagesCount = imagesCount;
        }

        private void LoadImageButton(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Image image = new Image();
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                source.UriSource = new Uri(openFileDialog.FileName, UriKind.RelativeOrAbsolute);

                source.EndInit();
                image.Source = source;
                ScrollViewer.Content = image;

            }
        }

        private void NextStepButton(object sender, RoutedEventArgs e)
        {
            // Export images
            string outputFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string randomFolderName = selectedGuid.ToString();
            string childFolderPath = System.IO.Path.Combine(outputFolder, "Temp", randomFolderName);
            Directory.CreateDirectory(childFolderPath);

            string folderName = System.IO.Path.GetFileName(randomFolderName);

            // Get the path to the selected image
            string selectedImagePath = GetSelectedImagePath();

            // Check if the source image file exists
            if (File.Exists(selectedImagePath))
            {
                // Convert BitmapImage to Bitmap and save the original image as BMP
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                source.UriSource = new Uri(selectedImagePath, UriKind.RelativeOrAbsolute);
                source.EndInit();

                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                string imageFileName = $"{folderName}.bmp";
                string destinationImagePath = System.IO.Path.Combine(childFolderPath, imageFileName);
                using (FileStream fs = new FileStream(destinationImagePath, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
            string jsonFilePath = System.IO.Path.Combine(childFolderPath, $"{folderName}.json");
            ScrollViewer.ExportRectanglesToJson(jsonFilePath, selectedImageIndex, ImagesCount);

            // Export images
            ScrollViewer.ExportImagesFromRectangles(childFolderPath, selectedImageIndex);

            // Pass data to a new window
            DisplayImagesWindow newWindow = new DisplayImagesWindow(childFolderPath, jsonFilePath, selectedImageIndex, selectedGuid);
            newWindow.Show();
        }

        private string GetSelectedImagePath()
        {
            if (ScrollViewer.Content is Image image)
            {
                if (image.Source is BitmapImage bitmapImage)
                {
                    return bitmapImage.UriSource.AbsolutePath;
                }
            }
            return null;
        }

    }
}