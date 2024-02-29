using FormBuilder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FormBuilder.CustomController
{
    public class ZoomableCanvas : Canvas, INotifyPropertyChanged
    {
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private Point _startPoint;
        private Rectangle _currentRectangle;
        private bool _isDrawing;
        private Slider _zoomSlider;
        private bool _isRectangleSelected;
        private double _rectangleOffsetX;
        private double _rectangleOffsetY;


        public event EventHandler<RectangleChangedEventArgs> RectangleChanged;
        public event EventHandler<RectangleDeletedEventArgs> RectangleDeleted;
        public event EventHandler<RectangleMovedEventArgs> RectangleMoved;
        public event EventHandler<RectangleSelectedEventArgs> RectangleSelected;
        public event EventHandler<RectangleSelectedEventArgs> SelectedZoneChanged;
        private double _backgroundImageWidth;
        private double _backgroundImageHeight;

        public double BackgroundImageWidth
        {
            get { return _backgroundImageWidth; }
            private set
            {
                if (_backgroundImageWidth != value)
                {
                    _backgroundImageWidth = value;
                    OnPropertyChanged(nameof(BackgroundImageWidth));
                }
            }
        }

        public double BackgroundImageHeight
        {
            get { return _backgroundImageHeight; }
            private set
            {
                if (_backgroundImageHeight != value)
                {
                    _backgroundImageHeight = value;
                    OnPropertyChanged(nameof(BackgroundImageHeight));
                }
            }
        }
        public Zone SelectedRectangle { get; private set; }

        public static readonly DependencyProperty BackgroundImageProperty =
     DependencyProperty.Register(
         "BackgroundImage",
         typeof(ImageSource),
         typeof(ZoomableCanvas),
         new PropertyMetadata(null, OnBackgroundImageChanged));

        public ImageSource BackgroundImage
        {
            get { return (ImageSource)GetValue(BackgroundImageProperty); }
            set { SetValue(BackgroundImageProperty, value); }
        }

        private static void OnBackgroundImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomableCanvas canvas = (ZoomableCanvas)d;
            canvas.UpdateBackgroundImage();

        }

        private void UpdateBackgroundImage()
        {
            if (BackgroundImage != null)
            {
                Image backgroundImageControl = new Image
                {
                    Source = BackgroundImage,
                    Stretch = Stretch.None
                };

                Children.Insert(0, backgroundImageControl);
                //backgroundImageControl.Width = ActualWidth;
                //backgroundImageControl.Height = ActualHeight;
            }
            _backgroundImageWidth = ActualWidth;
            _backgroundImageHeight = ActualHeight;
            OnPropertyChanged(nameof(BackgroundImageWidth));
            OnPropertyChanged(nameof(BackgroundImageHeight));
        }
   




        public static readonly DependencyProperty ZonesProperty =
     DependencyProperty.Register(
         "Zones",
         typeof(ObservableCollection<Zone>),
         typeof(ZoomableCanvas),
         new PropertyMetadata(new ObservableCollection<Zone>(), OnZonesChanged));

        public ObservableCollection<Zone> Zones
        {
            get { return (ObservableCollection<Zone>)GetValue(ZonesProperty); }
            set { SetValue(ZonesProperty, value); }
        }

        private static void OnZonesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomableCanvas canvas = (ZoomableCanvas)d;
            canvas.UpdateZones();
        }

        public double ScaleX { get; private set; } = 1.0;
        public double ScaleY { get; private set; } = 1.0;

    

        public Zone SelectedZone { get; private set; }
        private void UpdateZones()
        {
            // Clear existing rectangles from the canvas
            Children.Clear();

            // Create rectangles for each zone and add them to the canvas
            foreach (Zone zone in Zones)
            {
                Rectangle rectangle = new Rectangle
                {
                    Width = zone.Width,
                    Height = zone.Height,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(rectangle, zone.X);
                Canvas.SetTop(rectangle, zone.Y);

                Children.Add(rectangle);
            }
        }

        public ZoomableCanvas()
        {
            MouseWheel += ZoomableCanvas_MouseWheel;
            MouseLeftButtonDown += ZoomableCanvas_MouseLeftButtonDown;
            MouseMove += ZoomableCanvas_MouseMove;
            MouseLeftButtonUp += ZoomableCanvas_MouseLeftButtonUp;
            SizeChanged += ZoomableCanvas_SizeChanged;
            // Add zoom slider
            _zoomSlider = new Slider
            {
                Minimum = 0.1,
                Maximum = 10,
                Value = 1,
                Width = 100,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top
            };
            _zoomSlider.ValueChanged += ZoomSlider_ValueChanged;
            Children.Add(_zoomSlider);
            RectangleSelected += (sender, args) =>
            {
                SelectedZone = args.SelectedZone;
            };

            //Zones.CollectionChanged += Zones_CollectionChanged;
        }
        private void ZoomableCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBackgroundImage();
        }
        private void Zones_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateZones();
        }

        private void ZoomableCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Perform zooming based on the mouse wheel delta
            double zoomFactor = 1.1;
            if (e.Delta > 0)
                Zoom(zoomFactor, e.GetPosition(this));
            else
                Zoom(1 / zoomFactor, e.GetPosition(this));
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Perform zooming based on the slider value
            double zoomFactor = _zoomSlider.Value;
            Point zoomCenter = new Point(ActualWidth / 2, ActualHeight / 2);
            Zoom(zoomFactor, zoomCenter);
        }

        private void Zoom(double zoomFactor, Point zoomCenter)
        {
            // Scale the canvas and its children
            ScaleTransform scaleTransform = new ScaleTransform(zoomFactor, zoomFactor, zoomCenter.X, zoomCenter.Y);
            foreach (UIElement element in Children)
            {
                element.RenderTransformOrigin = new Point(0.5, 0.5);
                element.RenderTransform = scaleTransform;
            }

            // Update scaleX and scaleY properties
            ScaleX *= zoomFactor;
            ScaleY *= zoomFactor;
            _backgroundImageWidth = ActualWidth;
            _backgroundImageHeight = ActualHeight;
            OnPropertyChanged(nameof(BackgroundImageWidth));
            OnPropertyChanged(nameof(BackgroundImageHeight));
        }
        private void ZoomableCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isDrawing)
            {
                // Check if the click is within a rectangle
                foreach (UIElement element in Children)
                {
                    if (element is Rectangle rectangle && IsMouseOverRectangle(rectangle, e.GetPosition(this)))
                    {
                        // Select the rectangle for movement
                        _isRectangleSelected = true;
                        _currentRectangle = rectangle;

                        // Store the offset between the click point and the rectangle's top-left corner
                        _rectangleOffsetX = e.GetPosition(this).X - Canvas.GetLeft(_currentRectangle);
                        _rectangleOffsetY = e.GetPosition(this).Y - Canvas.GetTop(_currentRectangle);

                        // Break out of the loop since we found the selected rectangle
                        break;
                    }
                }

                // Start drawing a new rectangle if no rectangle is selected
                if (!_isRectangleSelected)
                {
                    _startPoint = e.GetPosition(this);
                    _currentRectangle = new Rectangle
                    {
                        Stroke = Brushes.Red,
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0))
                    };
                    Canvas.SetLeft(_currentRectangle, _startPoint.X);
                    Canvas.SetTop(_currentRectangle, _startPoint.Y);
                    Children.Add(_currentRectangle);
                    _isDrawing = true;
                }
            }
        }

        private void ZoomableCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing && _currentRectangle != null)
            {
                // Update the size of the current rectangle
                Point currentPoint = e.GetPosition(this);
                double width = Math.Abs(currentPoint.X - _startPoint.X);
                double height = Math.Abs(currentPoint.Y - _startPoint.Y);
                _currentRectangle.Width = width;
                _currentRectangle.Height = height;

                // Update the position of the current rectangle
                double newLeft = Math.Min(currentPoint.X, _startPoint.X);
                double newTop = Math.Min(currentPoint.Y, _startPoint.Y);
                Canvas.SetLeft(_currentRectangle, newLeft);
                Canvas.SetTop(_currentRectangle, newTop);

                if (width > 50 && height > 50)
                {
                    //BitmapSource croppedImage = CropSelectedRectangle(_currentRectangle);

                    RectangleMovedEventArgs args = new RectangleMovedEventArgs
                    {
                        SelectedZone = SelectedRectangle,
                       // CroppedImage = croppedImage,
                        X = newLeft,
                        Y = newTop,
                        Width = _currentRectangle.Width,
                        Height = _currentRectangle.Height
                    };

                    // Raise the RectangleMoved event with the cropped image and other information
                    RectangleMoved?.Invoke(this, new RectangleMovedEventArgs
                    {
                        SelectedZone = args.SelectedZone,
                       // CroppedImage = args.CroppedImage,
                        X = args.X,
                        Y = args.Y,
                        Width = args.Width,
                        Height = args.Height
                    });
                }
            }

            else if (_isRectangleSelected && _currentRectangle != null)
            {
                // Move the selected rectangle
                Point currentPosition = e.GetPosition(this);
                double newLeft = currentPosition.X - _rectangleOffsetX;
                double newTop = currentPosition.Y - _rectangleOffsetY;
                Canvas.SetLeft(_currentRectangle, newLeft);
                Canvas.SetTop(_currentRectangle, newTop);

                // Raise the RectangleMoved event with the updated rectangle information
               // BitmapSource croppedImage = CropSelectedRectangle(_currentRectangle);

                RectangleMovedEventArgs args = new RectangleMovedEventArgs
                {
                    SelectedZone = SelectedRectangle,
                 //   CroppedImage = croppedImage,
                    X = newLeft,
                    Y = newTop,
                    Width = _currentRectangle.Width,
                    Height = _currentRectangle.Height
                };

                // Raise the RectangleMoved event with the cropped image and other information
                RectangleMoved?.Invoke(this, new RectangleMovedEventArgs
                {
                    SelectedZone = args.SelectedZone,
                   // CroppedImage = args.CroppedImage,
                    X = args.X,
                    Y = args.Y,
                    Width = args.Width,
                    Height = args.Height
                });
            }
        }

        private void ZoomableCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawing && _currentRectangle != null)
            {
                // Finish drawing the rectangle and stop drawing
                _isDrawing = false;

                // Calculate the size of the fully drawn rectangle
                double width = Math.Abs(e.GetPosition(this).X - _startPoint.X);
                double height = Math.Abs(e.GetPosition(this).Y - _startPoint.Y);

                // Raise the RectangleChanged event with the fully drawn rectangle information
               // BitmapSource croppedImage = CropSelectedRectangle(_currentRectangle);

                RectangleChangedEventArgs args = new RectangleChangedEventArgs
                {
                    SelectedZone = null, // Set this to null for newly drawn rectangles
                    //CroppedImage = croppedImage,
                    X = Math.Min(e.GetPosition(this).X, _startPoint.X),
                    Y = Math.Min(e.GetPosition(this).Y, _startPoint.Y),
                    Width = width,
                    Height = height
                };

                // Raise the RectangleChanged event with the cropped image and other information
                RectangleChanged?.Invoke(this, new RectangleChangedEventArgs
                {
                    SelectedZone = args.SelectedZone,
                    //CroppedImage = args.CroppedImage,
                    X = args.X,
                    Y = args.Y,
                    Width = args.Width,
                    Height = args.Height
                });
            }
            else if (_isRectangleSelected && _currentRectangle != null)
            {
                // Raise the RectangleMoved event with the final position of the moved rectangle
                RectangleMovedEventArgs args = new RectangleMovedEventArgs
                {
                    X = Canvas.GetLeft(_currentRectangle),
                    Y = Canvas.GetTop(_currentRectangle),
                    Width = _currentRectangle.Width,
                    Height = _currentRectangle.Height
                };
                RectangleMoved?.Invoke(this, args);

                _isRectangleSelected = false;
                _currentRectangle = null;
            }
        }
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            // Check if a rectangle is clicked
            var clickedRectangle = e.Source as Rectangle;
            if (clickedRectangle != null && Children.Contains(clickedRectangle))
            {
                // Create a Zone object based on the clicked rectangle
                Zone selectedZone = new Zone
                {
                    X = Canvas.GetLeft(clickedRectangle),
                    Y = Canvas.GetTop(clickedRectangle),
                    Width = clickedRectangle.Width,
                    Height = clickedRectangle.Height
                };

                // Raise the RectangleSelected event with the selected rectangle
                RectangleSelectedEventArgs args = new RectangleSelectedEventArgs
                {
                    SelectedZone = selectedZone,
                    // CroppedImage = croppedImage
                };

                // Raise the RectangleSelected event with the cropped image and other information
                SelectedZoneChanged?.Invoke(this, new RectangleSelectedEventArgs
                {
                    SelectedZone = args.SelectedZone,
                    // CroppedImage = args.CroppedImage
                });
            }
        }
        private bool IsMouseOverRectangle(Rectangle rectangle, Point mousePosition)
        {
            double left = Canvas.GetLeft(rectangle);
            double top = Canvas.GetTop(rectangle);
            double right = left + rectangle.Width;
            double bottom = top + rectangle.Height;

            return mousePosition.X >= left && mousePosition.X <= right &&
                   mousePosition.Y >= top && mousePosition.Y <= bottom;
        }

        public List<Rectangle> GetRectangles()
        {
            List<Rectangle> rectangles = new List<Rectangle>();
            foreach (UIElement element in Children)
            {
                if (element is Rectangle rectangle)
                    rectangles.Add(rectangle);
            }
            return rectangles;
        }

        private BitmapSource CropSelectedRectangle(Rectangle rectangle)
        {
            if (rectangle == null || !(rectangle.Parent is ZoomableCanvas canvas))
            {
                return null;
            }

            // Calculate the position of the rectangle relative to the canvas
            double left = Canvas.GetLeft(rectangle);
            double top = Canvas.GetTop(rectangle);

            // Create a RenderTargetBitmap to render the ZoomableCanvas onto it
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)canvas.ActualWidth, (int)canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            // Render the ZoomableCanvas onto the RenderTargetBitmap
            renderTargetBitmap.Render(canvas);

            // Calculate the rectangle's position and size in device-independent units (1/96 inch)
            double dpiFactor = 96.0 / 96.0; // Assuming 96 DPI
            Int32Rect cropRect = new Int32Rect((int)(left * dpiFactor), (int)(top * dpiFactor),
                                               (int)(rectangle.Width * dpiFactor), (int)(rectangle.Height * dpiFactor));

            // Create a CroppedBitmap from the RenderTargetBitmap using the calculated crop rectangle
            CroppedBitmap croppedBitmap = new CroppedBitmap(renderTargetBitmap, cropRect);

            // Convert the CroppedBitmap to a BitmapSource (BitmapSource is a base class of CroppedBitmap)
            BitmapSource croppedImage = croppedBitmap as BitmapSource;

            return croppedImage;
        }


    }

    public class RectangleChangedEventArgs : EventArgs
    {
        public Zone SelectedZone { get; set; }
       // public BitmapSource CroppedImage { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class RectangleDeletedEventArgs : EventArgs
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class RectangleMovedEventArgs : EventArgs
    {
        public Zone SelectedZone { get; set; }
        //public BitmapSource CroppedImage { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class RectangleSelectedEventArgs : EventArgs
    {
        public Zone SelectedZone { get; set; }
       // public BitmapSource CroppedImage { get; set; }
    }
}
