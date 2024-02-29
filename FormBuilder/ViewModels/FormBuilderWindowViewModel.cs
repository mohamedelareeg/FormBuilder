using FormBuilder.CustomController;
using FormBuilder.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FormBuilder.ViewModels
{
    public class FormBuilderWindowViewModel : INotifyPropertyChanged
    {
        private ScrollViewer _scrollViewer;
        public ScrollViewer ScrollViewer
        {
            get { return _scrollViewer; }
            set
            {
                if (_scrollViewer != value)
                {
                    _scrollViewer = value;
                    OnPropertyChanged(nameof(ScrollViewer));
                }
            }
        }

        private Image _imageContent;
        public Image ImageContent
        {
            get { return _imageContent; }
            set
            {
                if (_imageContent != value)
                {
                    _imageContent = value;
                    OnPropertyChanged(nameof(ImageContent));
                }
            }
        }

        private ObservableCollection<Rect> _rectangles;
        public ObservableCollection<Rect> Rectangles
        {
            get { return _rectangles; }
            set
            {
                if (_rectangles != value)
                {
                    _rectangles = value;
                    OnPropertyChanged(nameof(Rectangles));
                }
            }
        }

        private ListBox _zoneListBox;
        public ListBox ZoneListBox
        {
            get { return _zoneListBox; }
            set
            {
                if (_zoneListBox != value)
                {
                    _zoneListBox = value;
                    OnPropertyChanged(nameof(ZoneListBox));
                }
            }
        }

        private Zone _currentZone;
        public Zone CurrentZone
        {
            get { return _currentZone; }
            set
            {
                if (_currentZone != value)
                {
                    _currentZone = value;
                    OnPropertyChanged(nameof(CurrentZone));
                }
            }
        }
        private ObservableCollection<Zone> _zones;
        public ObservableCollection<Zone> Zones
        {
            get { return _zones; }
            set
            {
                if (_zones != value)
                {
                    _zones = value;
                    OnPropertyChanged(nameof(Zones));
                }
            }
        }

        public FormBuilderWindowViewModel()
        {
            ImageContent = new Image();
            Rectangles = new ObservableCollection<Rect>();
            ScrollViewer = new ScrollViewer();

            //ImagePreview = new Image();
            CurrentZone = new Zone();
            Zones = new ObservableCollection<Zone>();
            ZoneListBox = new ListBox();
            ZoneListBox.ItemsSource = Zones;

            Image image = new Image();
            BitmapImage source = new BitmapImage();
            source.BeginInit();
            source.UriSource = new Uri("Clipboard01.png", UriKind.RelativeOrAbsolute);
            source.EndInit();
            image.Source = source;
            ScrollViewer.Content = image;

        }
        public ICommand DeleteZoneCommand => new RelayCommand(DeleteZone);

        private void DeleteZone(object parameter)
        {
            if (parameter is Zone zone)
            {
                Zones.Remove(zone);
                Zones.Remove(zone);
              
            }
        }

        public void ZoneListBox_SelectedZoneChanged(object sender, RectangleSelectedEventArgs e)
        {
           
        }
      
        public void ZoneListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ZoneListBox.SelectedItem != null)
            {
                Zone selectedZone = ZoneListBox.SelectedItem as Zone;
              
            }
        }
      
        public ICommand LoadImageButton => new RelayCommand(LoadImage);

        private void LoadImage(object obj)
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
        public ICommand SaveTemplateButton => new RelayCommand(SaveTemplate);

        private void SaveTemplate(object obj)
        {
            // Export images
            string outputFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string randomFolderName = Guid.NewGuid().ToString();
            string childFolderPath = System.IO.Path.Combine(outputFolder, randomFolderName);
            Directory.CreateDirectory(childFolderPath);
            ExportRectanglesToJson("rectangles.json");
            // Export images
            ExportImagesFromRectangles(outputFolder);
        }
        #region Export Template
        private void ExportRectanglesToJson(string filePath)
        {
            List<Rect> rectangles = new List<Rect>(Rectangles);

            // Convert the List<Rect> to a List<SerializableRect>
            List<Windows.ScrollViewer.SerializableRect> serializableRectangles = rectangles
                .Select(rect => new Windows.ScrollViewer.SerializableRect { X = rect.X, Y = rect.Y, Width = rect.Width, Height = rect.Height })
                .ToList();

            string json = JsonConvert.SerializeObject(serializableRectangles, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private void ExportImagesFromRectangles(string outputFolder)
        {
            List<Rect> rectangles = new List<Rect>(Rectangles);

            string randomParentFolderName = Guid.NewGuid().ToString();
            string parentFolderPath = System.IO.Path.Combine(outputFolder, randomParentFolderName);
            Directory.CreateDirectory(parentFolderPath);

            int imageIndex = 1;

            foreach (Rect rect in rectangles)
            {
                string fileName = $"image_{imageIndex}.png";
                string filePath = System.IO.Path.Combine(parentFolderPath, fileName);

                ExportImageFromRect(rect, filePath);
                imageIndex++;
            }
        }

        private void ExportImageFromRect(Rect rect, string filePath)
        {
            try
            {
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    VisualBrush visualBrush = new VisualBrush(ImageContent);
                    drawingContext.DrawRectangle(visualBrush, null, new Rect(-rect.X, -rect.Y, ImageContent.ActualWidth, ImageContent.ActualHeight));
                }

                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                    (int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(drawingVisual);

                PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    pngEncoder.Save(fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error exporting image: " + ex.Message);
            }
        }
        #endregion
        public ICommand LoadTemplateButton => new RelayCommand(LoadTemplate);

        private void LoadTemplate(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                LoadTemplateFile(filePath);
            }
        }

        private void LoadTemplateFile(string filePath)
        {
           
        }

        public ICommand SettingsButton => new RelayCommand(Settings);

        private void Settings(object obj)
        {
           
        }
      
        public ICommand FinishButton => new RelayCommand(Finish);

        private async void Finish(object obj)
        {

        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
