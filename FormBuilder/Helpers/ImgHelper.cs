using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FormBuilder.Helpers
{
    public static class ImgHelper
    {
        public static Bitmap CropBitmapImage(BitmapImage bitmapImage)
        {
            try
            {
                // Convert BitmapImage to Bitmap
                Bitmap bitmap = BitmapImage2Bitmap(bitmapImage);

                // Calculate the top crop height (e.g., top 100% of the image)
                int topCropHeight = bitmap.Height * 1;

                // Create a rectangle representing the top part of the image to crop
                Rectangle cropRect = new Rectangle(0, 0, bitmap.Width, topCropHeight);

                // Crop the bitmap
                Bitmap croppedBitmap = bitmap.Clone(cropRect, bitmap.PixelFormat);

                // Dispose the original bitmap
                bitmap.Dispose();



                // Return the cropped bitmap
                return croppedBitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while cropping the image: " + ex.Message);
                return null;
            }
        }
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            try
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    encoder.Save(outStream);
                    Bitmap bitmap = new Bitmap(outStream);
                    return new Bitmap(bitmap);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while converting BitmapImage to Bitmap: " + ex.Message);
                return null;
            }
        }
        public static Tesseract.Pix ImageToPix(Bitmap image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Save the Bitmap to a MemoryStream in BMP format
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

                // Rewind the MemoryStream
                ms.Seek(0, SeekOrigin.Begin);

                // Load the MemoryStream into a Pix object
                return Tesseract.Pix.LoadFromMemory(ms.GetBuffer());
            }
        }

        public static Bitmap PreProcessImage(Bitmap image)
        {
            if (image == null)
            {
                Console.WriteLine("Input image is null.");
                return null;
            }

            // Create a new Bitmap object for processing
            Bitmap processedImage = new Bitmap(image);

            // Convert the image to grayscale
            using (Graphics gr = Graphics.FromImage(processedImage))
            {
                // Create the grayscale color matrix
                ColorMatrix colorMatrix = new ColorMatrix(
                    new float[][]
                    {
                new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
                new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 1 }
                    });

                // Create the ImageAttributes object and set the color matrix
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                // Draw the image using the grayscale color matrix
                gr.DrawImage(
                    processedImage,
                    new Rectangle(0, 0, processedImage.Width, processedImage.Height),
                    0, 0, processedImage.Width, processedImage.Height,
                    GraphicsUnit.Pixel,
                    attributes
                );
            }

            // Perform any additional image processing steps here

            return processedImage;
        }


    }
}
