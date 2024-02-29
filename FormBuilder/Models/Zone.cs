using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FormBuilder.Models
{
    [Serializable]
    public class Zone : INotifyPropertyChanged
    {
        private double x;
        private double y;
        private double width;
        private double height;

        [JsonIgnore]
        public BitmapSource? CroppedImage { get; set; }
        public string Name { get; set; }
        public string IndexingField { get; set; }
        public string Regex { get; set; }

        public string Type { get; set; }

        public double X
        {
            get => x;
            set
            {
                x = value;
                OnPropertyChanged(nameof(X));
            }
        }
        public double Y
        {
            get => y;
            set
            {
                y = value;
                OnPropertyChanged(nameof(Y));
            }
        }
        public double Width
        {
            get => width;
            set
            {
                width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
        public double Height
        {
            get => height;
            set
            {
                height = value;
                OnPropertyChanged(nameof(Height));
            }
        }

        private Delegate[] GetEventHandlers(string eventName)
        {
            PropertyChangedEventHandler eventHandler = PropertyChanged;
            if (eventHandler == null)
                return null;

            Delegate[] delegates = eventHandler.GetInvocationList();
            return delegates;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
