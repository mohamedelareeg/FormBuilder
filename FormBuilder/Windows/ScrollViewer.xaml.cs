using FormBuilder.CustomController;
using Newtonsoft.Json;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FormBuilder.Windows
{
    /// <summary>
    /// Interaction logic for ScrollViewer.xaml
    /// </summary>
	public partial class ScrollViewer : UserControl
	{
		public  System.Windows.FrameworkElement Content
		{
			get => (System.Windows.FrameworkElement)GridContent.Children[0];
			set 
			{
				GridContent.Children.Clear();
				GridContent.Children.Add(value);
				Rect view = new Rect(0, 0, value.ActualWidth, value.ActualHeight);
				ViewArea = view;
				value.SizeChanged += Value_SizeChanged;
			}
		}

		public Item AddItem(Rect rect)
		{
			Item item = new Item();
			item.Rect = rect;
			item.ScrollViewer = this;
			GridItem.Children.Add(item);
			SelectedItem = item;
			return item;
		}

		public List<Rect> GetAllRects()
		{
			List<Rect> result = new List<Rect>();
			foreach (Item item in GridItem.Children)
			{
				result.Add(item.Rect);
			}

			return result;
		}

		private void Value_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			//ZoomInFull();
		}

		Point? lastCenterPositionOnTarget;
		Point? lastMousePositionOnTarget;
		Point? lastDragPoint;

		public ScrollViewer()
		{
            InitializeComponent();

            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;

            GridContent.MouseLeftButtonDown += OnMouseLeftButtonDown;
            GridContent.MouseMove += OnMouseMove;
            scrollViewer.MouseMove += ScrollViewer_MouseMove;
            scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseRightButtonDown += ScrollViewer_PreviewMouseRightButtonDown;
            scrollViewer.PreviewMouseRightButtonUp += ScrollViewer_PreviewMouseRightButtonUp;
            scrollViewer.KeyDown += ScrollViewer_KeyDown;

            slider.ValueChanged += OnSliderValueChanged;

            ZoomInFull(); 
			
			

			foreach (Item item in GridItem.Children)
			{
				item.ScrollViewer = this;
			}
		}

		private void ScrollViewer_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete || e.Key == Key.Back)
			{
				if (SelectedItem != null)
				{
					GridItem.Children.Remove(SelectedItem);
					SelectedItem = null;
				}
			}
		}

		//creating
		Point creatingStart;
		private void ScrollViewer_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			MoveMode = MoveModes.Creating;
			creatingStart = e.GetPosition(GridContent);
			Item nuItem = AddItem(new Rect(0, 0, 0, 0));
			MouseButtonDownHandler(nuItem, e);
		}

		private void ScrollViewer_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			scrollViewer.ReleaseMouseCapture();
			lastDragPoint = null;
			MoveMode = MoveModes.None;
		}


		private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
		{
			if (lastDragPoint.HasValue)
			{
				if (moveMode == MoveModes.MoveAll)
				{
					Point posNow = e.GetPosition(scrollViewer);

					double dX = posNow.X - lastDragPoint.Value.X;
					double dY = posNow.Y - lastDragPoint.Value.Y;

					lastDragPoint = posNow;

					//scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
					//scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);

					Rect rect = ViewArea;

					rect.X -= dX / Scale;
					rect.Y -= dY / Scale;

					ViewArea = rect;

					Point pos = e.GetPosition(GridContent);
				}
				else if (moveMode == MoveModes.Creating)
				{
					Point posNow = e.GetPosition(GridContent);

					double x = Math.Min(creatingStart.X, posNow.X);
					double y = Math.Min(creatingStart.Y, posNow.Y);
					double width = Math.Abs(creatingStart.X - posNow.X);
					double height = Math.Abs(creatingStart.Y - posNow.Y);

					SelectedItem.Rect = new Rect(x, y, width, height);
				}
				else if (SelectedItem != null)
				{
					SelectedItem.Move(e, (int)moveMode);
				}
			}
			else
			{
				
			}
		}

		

		Rect viewArea = new Rect();

		public double Scale
		{
			get => scaleTransform.ScaleX;
			set
			{
				scaleTransform.ScaleX = value;
				scaleTransform.ScaleY = value;

				if (SelectedItem != null)
				{
					SelectedItem.UiWidth = value;
				}
			}
		}

		public Rect ViewArea
		{
			set
			{
				double windowWidth = scrollViewer.ViewportWidth;
				double windowHeight = scrollViewer.ViewportHeight;
				double windowRate = windowWidth / windowHeight;

				if (windowWidth == 0)
				{
					windowWidth = scrollViewer.ActualWidth;
					windowHeight = scrollViewer.ActualHeight;
				}

				double a = GridContent.Width;

				//double contentWidth = scrollViewer.ExtentWidth;
				//double contentHeight = scrollViewer.ExtentHeight; 
				double contentWidth = grid.ActualWidth;
				double contentHeight = grid.ActualHeight;
				double contentRate = contentWidth / contentHeight;

				//oriented in content.
				Rect rect = value;

				if (rect.Width == 0 || contentWidth == 0 || windowWidth == 0)
				{
					viewArea = rect;
					return;
				}

				//--decide scale
				//allowed by scrollViewer
				double minScale = Math.Min(windowWidth / contentWidth, windowHeight / contentHeight);
				

				double scaleX = Math.Max(windowWidth / rect.Width, minScale);
				double scaleY = Math.Max(windowHeight / rect.Height, minScale);

				double scale;
				//(x or y) axis should be extended.
				if (scaleX > scaleY)
				{
					scale = scaleY;
					double oldWidth = rect.Width;
					rect.Width = windowWidth / scale;
					rect.X -= (rect.Width - oldWidth) / 2;//extend from center
				}
				else
				{
					scale = scaleX;
					double oldHeight = rect.Height;
					rect.Height = windowHeight / scale;
					rect.Y -= (rect.Height - oldHeight) / 2;
				}

				Scale = scale;

				//double extendedWidth = contentWidth * scale;
				//double extendedHeight = contentHeight * scale;

				scrollViewer.ScrollToHorizontalOffset(rect.X * scale);
				scrollViewer.ScrollToVerticalOffset(rect.Y * scale);

				//viewArea = rect;
			}

			get
			{
				return viewArea;
			}
		}

		void ZoomInFull()
		{
			ViewArea = new Rect(0, 0, GridContent.ActualWidth, GridContent.ActualHeight);
		}

		void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (lastDragPoint.HasValue)
			{
				Point posNow = e.GetPosition(scrollViewer);

				double dX = posNow.X - lastDragPoint.Value.X;
				double dY = posNow.Y - lastDragPoint.Value.Y;

				lastDragPoint = posNow;

				//scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
				//scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);

				Rect rect = ViewArea;

				rect.X -= dX / Scale;
				rect.Y -= dY / Scale;

				ViewArea = rect;

				Point pos = e.GetPosition(GridContent);
			}
			else
			{
				MoveMode = MoveModes.MoveAll;
			}
		}

		void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			MouseButtonDownHandler(null, e);
			//Mouse.Capture(scrollViewer);
		}

		void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			double scale = 1;
			if (e.Delta > 0)
			{
				scale /= 1.2;
			}
			if (e.Delta < 0)
			{
				scale *= 1.2;
			}

			lastMousePositionOnTarget = Mouse.GetPosition(grid);

			Point pos = e.GetPosition(GridContent);

			Rect view = ViewArea;

			double nuWidth = view.Width * scale;
			double nuHeight = view.Height * scale;

			// leftSide / total width
			double rateX = (pos.X - view.X) / view.Width;
			view.X -= (nuWidth - view.Width) * rateX;

			//topSide / total height
			double rateY = (pos.Y - view.Y) / view.Height;
			view.Y -= (nuHeight - view.Height) * rateY;

			view.Width = nuWidth;
			view.Height = nuHeight;

			ViewArea = view;
		}

		void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			scrollViewer.ReleaseMouseCapture();
			lastDragPoint = null;
		}

		void OnSliderValueChanged(object sender,
			 RoutedPropertyChangedEventArgs<double> e)
		{
			Scale = e.NewValue;

			var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2,
											 scrollViewer.ViewportHeight / 2);
			lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, grid);
		}

		void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			double scale = Scale;
			if (double.IsNaN(scale))
			{
				//scale = 1;
			}

			
			if (scale != 0)
			{
				viewArea.X = scrollViewer.HorizontalOffset / scale;
				viewArea.Y = scrollViewer.VerticalOffset / scale;
				viewArea.Width = scrollViewer.ViewportWidth / scale;
				viewArea.Height = scrollViewer.ViewportHeight / scale;

				double contentWidth = GridContent.ActualWidth;
				double contentHeight = GridContent.ActualHeight;

				if (viewArea.Width > contentWidth)
				{
					viewArea.X -= (viewArea.Width - contentWidth) / 2;
				}

				if (viewArea.Height > contentHeight)
				{
					viewArea.Y -= (viewArea.Height - contentHeight) / 2;
				}
			}
		}

		Item selectedItem = null;
		public Item SelectedItem
		{
			get
			{
				return selectedItem;
			}

			set
			{
				if (selectedItem != null)
				{
					selectedItem.Selected = false;
				}

				if (value != null)
				{
					value.UiWidth = Scale;
					value.Selected = true;
				}

				selectedItem = value;
			}
		}

		public enum MoveModes : int
		{
			LeftTop = 0,
			Top = 1,
			RightTop = 2,
			Left = 3,
			Right = 4,
			LeftBottom = 5,
			Bottom = 6,
			RightBottom = 7,
			MoveSelected = 8,

			MoveAll,
			None,
			Creating
		}

		MoveModes moveMode;
		public MoveModes MoveMode
		{
			set
			{
				if (lastDragPoint.HasValue)
				{
					return;
				}

				Console.WriteLine(value.ToString());
				if (value == MoveModes.LeftTop)
				{
					Cursor = Cursors.SizeNWSE;
				}
				else if (value == MoveModes.Top)
				{
					Cursor = Cursors.SizeNS;
				}
				else if (value == MoveModes.RightTop)
				{
					Cursor = Cursors.SizeNESW;
				}
				else if (value == MoveModes.Left)
				{
					Cursor = Cursors.SizeWE;
				}
				else if (value == MoveModes.Right)
				{
					Cursor = Cursors.SizeWE;
				}
				else if (value == MoveModes.LeftBottom)
				{
					Cursor = Cursors.SizeNESW;
				}
				else if (value == MoveModes.Bottom)
				{
					Cursor = Cursors.SizeNS;
				}
				else if (value == MoveModes.RightBottom)
				{
					Cursor = Cursors.SizeNWSE;
				}
				else if (value == MoveModes.MoveSelected)
				{
					Cursor = Cursors.SizeAll;
				}
				else 
				{
					Cursor = Cursors.Arrow;
				}
				moveMode = value;
			}

			get
			{
				return moveMode;
			}
		}

		public void MouseButtonDownHandler(Item sender, MouseEventArgs e)
		{
			SelectedItem = sender;

			var mousePos = e.GetPosition(scrollViewer);
			if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y <
				scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
			{
				lastDragPoint = mousePos;
				Mouse.Capture(scrollViewer);
			}
		}
        public class SerializableRect
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
			public double ActualWidth { get; set; }
			public double ActualHeight { get; set; }

            public string ImageFileName { get; set; }
            public string Name { get; set; }
            public string IndexingField { get; set; }
            public string Regex { get; set; } 
            public string Type { get; set; }
			public string WhiteList { get; set; }
			public bool IsDuplicated { get; set; } = false;
        }
        public class FormZones
        {
            public int Count { get; set; }
			public bool IsDuplicated { get; set; } = false;
            public List<TemplateImage> TemplateImages { get; set; }
        }

        public class TemplateImage
        {
            public int Index { get; set; }
            public List<SerializableRect> SerializableRect { get; set; }

        }

        public void ExportRectanglesToJson(string filePath, int imageIndex , int ImageCount)
        {
            List<Rect> rectangles = GetAllRects();

            TemplateImage templateImage = new TemplateImage
            {
                Index = imageIndex,
                SerializableRect = rectangles
                    .Select((rect, index) =>
                        new SerializableRect
                        {
                            X = rect.X,
                            Y = rect.Y,
                            ActualHeight = Content.ActualHeight,
                            ActualWidth = Content.ActualWidth,
                            Width = rect.Width,
                            Height = rect.Height,
                            ImageFileName = $"image_{index + 1}.png"
                        })
                    .ToList()
            };

            FormZones form;
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                form = JsonConvert.DeserializeObject<FormZones>(jsonData);
            }
            else
            {
                form = new FormZones() {Count = ImageCount };
            }
            if (form.TemplateImages == null)
            {
                form.TemplateImages = new List<TemplateImage>();
            }
            // Find if an entry with the same count already exists in the list
            TemplateImage existingTemplateImage = form.TemplateImages.FirstOrDefault(img => img.Index == imageIndex);
            if (existingTemplateImage != null)
            {
                // Replace the existing entry with the new data
                form.TemplateImages.Remove(existingTemplateImage);
            }

            // Add the new templateImage
            form.TemplateImages.Add(templateImage);

            // Serialize the modified form and save it to the file
            string json = JsonConvert.SerializeObject(form, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }



        public void ExportImagesFromRectangles(string outputFolder , int templateImageIndex)
        {
            List<Rect> rectangles = GetAllRects();
			string outputTemplateImage = System.IO.Path.Combine(outputFolder, templateImageIndex.ToString());

            Directory.CreateDirectory(outputTemplateImage);

            int imageIndex = 1;

            foreach (Rect rect in rectangles)
            {
                string fileName = $"image_{imageIndex}.png";
                string filePath = System.IO.Path.Combine(outputTemplateImage, fileName);

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
                    VisualBrush visualBrush = new VisualBrush(Content);
                    drawingContext.DrawRectangle(visualBrush, null, new Rect(-rect.X, -rect.Y, Content.ActualWidth, Content.ActualHeight));
                }

                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                    (int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(drawingVisual);


                // Use TIFF for lossless quality preservation
                TiffBitmapEncoder tiffEncoder = new TiffBitmapEncoder();
                tiffEncoder.Compression = TiffCompressOption.None;
                tiffEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    tiffEncoder.Save(fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error exporting image: " + ex.Message);
            }
        }

     
    }
}
