using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Tesseract;

namespace FormBuilder.Helpers
{
    public static class OcrHelper
    {
        private static string tessdataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
        private static string Language = "eng";//ara+
        private static TesseractEngine _engine = new TesseractEngine(tessdataPath, Language, EngineMode.Default);


        public static async Task<string> PerformOCRAsync(BitmapImage bitmap)
        {
            try
            {
                // Convert BitmapImage to Bitmap
                Bitmap croppedBitmap = await Task.Run(() => ImgHelper.CropBitmapImage(bitmap));

                // Perform OCR using the OcrHelper class
                string recognizedText = await Task.Run(() => PerformOCR(croppedBitmap));
                return recognizedText;
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., log or display an error message
                Console.WriteLine("Error in PerformOCRAsync: " + ex.Message);
                return string.Empty;
            }
        }

        public static string PerformOCR(Bitmap image)
        {
            try
            {
                // Pre-process the image using OpenCV
                using (var processedImage = ImgHelper.PreProcessImage(image))
                {
                    // Extract the fields from the template region
                    using (var page = _engine.Process(ImgHelper.ImageToPix(processedImage)))
                    {
                        return page.GetText();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., log or display an error message
                Console.WriteLine("Error in PerformOCR: " + ex.Message);
                return string.Empty;
            }
        }
    }
}
