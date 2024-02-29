using FormBuilder.Helpers;
using FormBuilder.Models;
using FormBuilder.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static FormBuilder.Windows.ScrollViewer;


namespace FormBuilder
{
    public class DisplayImagesViewModel : INotifyPropertyChanged
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
        private int _imageCount;
        public int ImageCount
        {
            get { return _imageCount; }
            set
            {
                if (_imageCount != value)
                {
                    _imageCount = value;
                    OnPropertyChanged(nameof(ImageCount));
                }
            }
        }
        private int _currentImageIndex;
        public int CurrentImageIndex
        {
            get { return _currentImageIndex; }
            set
            {
                if (_currentImageIndex != value)
                {
                    _currentImageIndex = value;
                    OnPropertyChanged(nameof(CurrentImageIndex));
                }
            }
        }

        private ObservableCollection<ImageData> zonesList;
        private ImageData currentZone;
        private string selectedImage;

        public ObservableCollection<ImageData> ZonesList
        {
            get { return zonesList; }
            set
            {
                if (zonesList != value)
                {
                    zonesList = value;
                    OnPropertyChanged(nameof(ZonesList));
                }
            }
        }

        public ImageData CurrentZone
        {
            get { return currentZone; }
            set
            {
                if (currentZone != value)
                {
                    currentZone = value;
                    OnPropertyChanged(nameof(CurrentZone));
                    UpdateSelectedImage();
                }
            }
        }

        public string SelectedImage
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

        public ICommand ModifyButton { get; private set; }
        public ICommand DeleteButton { get; private set; }
        public ICommand FinishButton { get; private set; }

        private string imagesFolderPath;
        private string jsonFilePath;
        public DisplayImagesViewModel(string imagesFolderPath , string jsonFilePath, int _selectedImageIndex, Guid _selectedGuid)
        {
            this.CurrentImageIndex = _selectedImageIndex;
            this.FormId = _selectedGuid;
            this.imagesFolderPath = imagesFolderPath;
            this.jsonFilePath = jsonFilePath;
            loadAllZones();
            InitializeCommands();
            
          
        }
        private void loadAllZones()
        {
            try
            {
                if (File.Exists(jsonFilePath))
                {
                    string jsonData = File.ReadAllText(jsonFilePath);
                    FormZones form = JsonConvert.DeserializeObject<FormZones>(jsonData);

                    ImageCount = form.Count;

                    TemplateImage currentTemplateImage = form.TemplateImages.FirstOrDefault(img => img.Index == CurrentImageIndex);

                    if (currentTemplateImage != null)
                    {
                        List<DisplayImagesViewModel.ImageData> imageList = new List<DisplayImagesViewModel.ImageData>();

                        foreach (SerializableRect rect in currentTemplateImage.SerializableRect)
                        {
                            string imageFileName = $"image_{imageList.Count + 1}.png";
                            DisplayImagesViewModel.ImageData imageData = new DisplayImagesViewModel.ImageData
                            {
                                ImageFileName = imageFileName,
                                Rect = rect
                            };
                            imageList.Add(imageData);
                        }

                        ZonesList = new System.Collections.ObjectModel.ObservableCollection<DisplayImagesViewModel.ImageData>(imageList);
                    }
                    else
                    {
                        MessageBox.Show("No template image found with the specified index.");
                    }

                }
                else
                {
                    MessageBox.Show("JSON file not found: " + jsonFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data from the .json file: " + ex.Message);
            }
        }



        /*
        private void loadAllZones()
        {
            try
            {
                string jsonData = File.ReadAllText(jsonFilePath);
                List<SerializableRect> rectangles = JsonConvert.DeserializeObject<List<SerializableRect>>(jsonData);
                List<DisplayImagesViewModel.ImageData> imageList = new List<DisplayImagesViewModel.ImageData>();

                // Iterate through the rectangles and extract image names
                foreach (SerializableRect rect in rectangles)
                {
                    string imageFileName = $"image_{imageList.Count + 1}.png";
                    DisplayImagesViewModel.ImageData imageData = new DisplayImagesViewModel.ImageData
                    {
                        ImageFileName = imageFileName,
                        Rect = rect
                    };
                    imageList.Add(imageData);
                }

                ZonesList = new System.Collections.ObjectModel.ObservableCollection<DisplayImagesViewModel.ImageData>(imageList);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data from the .json file: " + ex.Message);
            }
        }
        */

        private void InitializeCommands()
        {
            ModifyButton = new RelayCommand(ModifyZone);
            DeleteButton = new RelayCommand(DeleteZone);
            FinishButton = new RelayCommand(ExportData);
        }
        private void ModifyZone(object parameter)
        {
           
        }

        private void DeleteZone(object parameter)
        {
            if (CurrentZone != null)
            {
                ZonesList.Remove(CurrentZone);
                CurrentZone = null;
            }
        }

        private void ExportData(object parameter)
        {
            // Get the folder path where the images and the original JSON file are located
            string folderPath = System.IO.Path.GetDirectoryName(jsonFilePath);

            // Get the folder name from the original JSON file path
            string folderName = System.IO.Path.GetFileNameWithoutExtension(jsonFilePath);
            // Generate the new JSON file path with the same folder name
            string oldJsonFilePath = System.IO.Path.Combine(folderPath, folderName + ".json");
            string newJsonFilePath = System.IO.Path.Combine(folderPath, "form.json");
            //SaveDataToJson();
            ExportJsonWithNewFormat(newJsonFilePath);
            UpdateJsonFile(oldJsonFilePath);
            MessageBox.Show("Exported Successfully");
        }
        /*
        private void SaveCanvasWithRectanglesAsPng(string imageFilePath, string filePath, ObservableCollection<ImageData> zonesList)
        {
            try
            {
                // Save the canvas with children rectangles as PNG
                string canvasFileName = System.IO.Path.GetFileNameWithoutExtension(filePath) + "_with_rectangles.png";
                string canvasFilePath = System.IO.Path.Combine(imagesFolderPath, canvasFileName);

                using (FileStream fileStream = new FileStream(canvasFilePath, FileMode.Create))
                {
                    BitmapImage backgroundImage = new BitmapImage(new Uri(imageFilePath));

                    DrawingVisual drawingVisual = new DrawingVisual();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {
                        drawingContext.DrawImage(backgroundImage, new Rect(0, 0, backgroundImage.PixelWidth, backgroundImage.PixelHeight));

                        foreach (var zone in zonesList)
                        {
                            System.Windows.Rect rect = new System.Windows.Rect(zone.Rect.X, zone.Rect.Y, zone.Rect.Width, zone.Rect.Height);
                            drawingContext.DrawRectangle(System.Windows.Media.Brushes.Red, null, rect);
                        }
                    }

                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                        backgroundImage.PixelWidth,
                        backgroundImage.PixelHeight,
                        backgroundImage.DpiX,
                        backgroundImage.DpiY,
                        PixelFormats.Pbgra32);
                    renderTargetBitmap.Render(drawingVisual);

                    PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                    pngEncoder.Save(fileStream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving canvas with rectangles as PNG: " + ex.Message);
            }
        }
        */
        private void UpdateJsonFile(string filePath)
        {
            try
            {
                // Read the existing JSON data from the file
                string existingJsonData = File.ReadAllText(filePath);

                // Deserialize the existing JSON data into a Form object
                var existingForm = JsonConvert.DeserializeObject<FormZones>(existingJsonData);

                // Find the index of the current templateImage (if it exists)
                int currentTemplateIndex = -1;
                for (int i = 0; i < existingForm.TemplateImages.Count; i++)
                {
                    if (existingForm.TemplateImages[i].Index == CurrentImageIndex)
                    {
                        currentTemplateIndex = i;
                        break;
                    }
                }

                // Update the current templateImage or create a new one if not found
                var rectangles = new List<SerializableRect>();
                foreach (var imageData in ZonesList)
                {
                    rectangles.Add(imageData.Rect);
                }

                var updatedTemplateImage = new TemplateImage
                {
                    Index = CurrentImageIndex,
                    SerializableRect = rectangles
                };

                if (currentTemplateIndex != -1)
                {
                    existingForm.TemplateImages[currentTemplateIndex] = updatedTemplateImage;
                }
                else
                {
                    existingForm.TemplateImages.Add(updatedTemplateImage);
                }

                // Serialize the updated Form object to JSON
                string updatedJsonData = JsonConvert.SerializeObject(existingForm, Formatting.Indented);

                // Save the updated JSON data back to the file
                File.WriteAllText(filePath, updatedJsonData);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating data in the .json file: " + ex.Message);
            }
        }
        private void ExportJsonWithSameFormat(string filePath)
        {
            try
            {
                // Convert ZonesList to a list of SerializableRect objects
                var rectangles = new List<SerializableRect>();
                foreach (var imageData in ZonesList)
                {
                    rectangles.Add(imageData.Rect);
                }

                // Create the TemplateImage object
                var templateImage = new TemplateImage
                {
                    Index = CurrentImageIndex,
                    SerializableRect = rectangles
                };

                // Create the Form object
                var form = new FormZones
                {
                    Count = ImageCount,
                    TemplateImages = new List<TemplateImage> { templateImage }
                };

                // Serialize the object to JSON
                string jsonData = JsonConvert.SerializeObject(form, Formatting.Indented);

                // Save the JSON data to the file
                File.WriteAllText(filePath, jsonData);
        
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting data to the .json file: " + ex.Message);
            }
        }
        private void ExportJsonWithNewFormat(string filePath)
        {
            try
            {
                // Convert ZonesList to a list of SerializableRect objects
                var rectangles = new List<SerializableRect>();
                foreach (var imageData in ZonesList)
                {
                    rectangles.Add(imageData.Rect);
                }

                // Get the folder name from the original JSON file path
                string folderName = System.IO.Path.GetFileNameWithoutExtension(jsonFilePath);

                // Create the ImageDataWithZones object
                var imageDataWithZones = new ImageDataWithZones
                {
                    ImageFileName = $"{folderName}.bmp",
                    Zones = rectangles
                };

                // Serialize the object to JSON
                string jsonData = JsonConvert.SerializeObject(imageDataWithZones, Formatting.Indented);

                // Save the JSON data to the file
                File.WriteAllText(filePath, jsonData);
             
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting data to the .json file: " + ex.Message);
            }
        }

        private void SaveDataToJson()
        {
            try
            {
                // Convert ZonesList to a list of SerializableRect objects
                var rectangles = new List<SerializableRect>();
                foreach (var imageData in ZonesList)
                {
                    rectangles.Add(imageData.Rect);
                }

                // Get the folder name from the original JSON file path
                string folderName = System.IO.Path.GetFileNameWithoutExtension(jsonFilePath);

                // Create the TemplateImage object
                var templateImage = new TemplateImage
                {
                    Index = CurrentImageIndex,
                    SerializableRect = rectangles
                };

                // Create the Form object
                var form = new FormZones
                {
                    Count = ImageCount,
                    TemplateImages = new List<TemplateImage> { templateImage }
                };

                // Serialize the object to JSON
                string jsonData = JsonConvert.SerializeObject(form, Formatting.Indented);

                // Get the folder path where the images and the original JSON file are located
                string folderPath = System.IO.Path.GetDirectoryName(jsonFilePath);

                // Generate the new JSON file path with the same folder name
                string newJsonFilePath = System.IO.Path.Combine(folderPath, folderName + ".json");

                // Save the JSON data to the new file
                File.WriteAllText(newJsonFilePath, jsonData);
                MessageBox.Show("Exported Successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data to the .json file: " + ex.Message);
            }
        }




        private void UpdateSelectedImage()
        {
            if (CurrentZone != null)
            {
                string imagePath = System.IO.Path.Combine(imagesFolderPath, CurrentImageIndex.ToString(), CurrentZone.ImageFileName);
                SelectedImage = imagePath;
            }
        }

        public  void ZonesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {

           
            if (CurrentZone is DisplayImagesViewModel.ImageData selectedImage)
            {
                string imagePath = System.IO.Path.Combine(imagesFolderPath, CurrentImageIndex.ToString(), selectedImage.ImageFileName);
            
                SelectedImage = imagePath;
                // Load the image from imagePath using BitmapImage
                BitmapImage imageSource = new BitmapImage(new Uri(imagePath, UriKind.Absolute));

                // Convert BitmapImage to Bitmap
                Bitmap bitmap = BitmapImage2Bitmap(imageSource);

                // Now you can use the 'bitmap' for OCR
                string ocrText =  OcrHelper.PerformOCR(bitmap);
                ZoneWindow zoneWindow = new ZoneWindow(ocrText , CurrentZone);
                if (zoneWindow.ShowDialog() == true)
                {
                    CurrentZone.Rect.IndexingField = zoneWindow.IndexingField;
                    CurrentZone.Rect.Regex = zoneWindow.ValueRegexPattern;
                    CurrentZone.Rect.Type = zoneWindow.SelectedType;


                }
            }
            }
            catch (Exception)
            {

             
            }
        }
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(memoryStream);
                return new Bitmap(memoryStream);
            }
        }
        public void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {


                if (CurrentZone is DisplayImagesViewModel.ImageData selectedImage)
                {
                    string imagePath = System.IO.Path.Combine(imagesFolderPath , CurrentImageIndex.ToString(), selectedImage.ImageFileName);
                    SelectedImage = imagePath;
                }
            }
            catch (Exception)
            {


            }
        }

        // Implement the INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class ImageData
        {
            public string ImageFileName { get; set; }
            public SerializableRect Rect { get; set; }
        }
        public class ImageDataWithZones
        {
            public string ImageFileName { get; set; }
            public List<SerializableRect> Zones { get; set; }
        }
    }
}
