using FormBuilder.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace FormBuilder.Windows
{
    /// <summary>
    /// Interaction logic for ImageSelectorWindow.xaml
    /// </summary>
    public partial class ImageSelectorWindow : Window
    {
        ImageGalleryViewModel viewModel;
        //Form _SelectedForm = new Form();
        public ImageSelectorWindow()//Form selectedForm
        {
            //_SelectedForm = selectedForm;
            InitializeComponent();
            viewModel = new ImageGalleryViewModel();
            DataContext = viewModel;
            Guid newFormId = Guid.NewGuid();
            viewModel.FormId = newFormId;
        }
        private void LoadImageButton(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.tif;*.gif;*.bmp|All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string fileName in openFileDialog.FileNames)
                {
                    BitmapImage imageSource = new BitmapImage(new Uri(fileName));
                    ImageItem imageItem = new ImageItem
                    {
                        Name = System.IO.Path.GetFileName(fileName),
                        Image = imageSource
                    };
                    viewModel.LoadedImages.Add(imageItem);
                }
            }
        }


        private void NextStepButton(object sender, RoutedEventArgs e)
        {
            if (ImageListBox.SelectedItem is ImageItem selectedImage)
            {
                int selectedImageIndex = viewModel.LoadedImages.IndexOf(selectedImage);
                MainWindow mainWindow = new MainWindow(selectedImage, selectedImageIndex , viewModel.FormId , viewModel.LoadedImages.Count());
                mainWindow.Show();
            }
        }

        private void ImageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.ImageListBox_SelectionChanged(sender, e);

        }
    }
    public class ImageItem
    {
        public string Name { get; set; }
        public BitmapImage Image { get; set; }
    }
    public class ImageGalleryViewModel : INotifyPropertyChanged
    {
        private Guid formId;
        public Guid FormId
        {
            get { return formId; }
            set
            {
                if (formId != value)
                {
                    formId = value;
                    OnPropertyChanged(nameof(FormId));
                }
            }
        }
      
        public ImageGalleryViewModel()
        {
            loadedImages = new ObservableCollection<ImageItem>();
            selectedImage = new ImageItem();
        }
        private ObservableCollection<ImageItem> loadedImages = new ObservableCollection<ImageItem>();
        public ObservableCollection<ImageItem> LoadedImages
        {
            get { return loadedImages; }
            set
            {
                if (loadedImages != value)
                {
                    loadedImages = value;
                    OnPropertyChanged(nameof(LoadedImages));
                }
            }
        }

        private ImageItem selectedImage;
        public ImageItem SelectedImage
        {
            get { return selectedImage; }
            set
            {
                if (selectedImage != value)
                {
                    selectedImage = value;
                    OnPropertyChanged(nameof(SelectedImage));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ImageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SelectedImage = (ImageItem)e.AddedItems[0];
            }
        }

    }
}
