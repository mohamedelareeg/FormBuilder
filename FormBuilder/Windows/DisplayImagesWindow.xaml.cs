
using FormBuilder.Models;
using FormBuilder.ViewModels;
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
using System.Windows.Shapes;

namespace FormBuilder.Windows
{
    /// <summary>
    /// Interaction logic for DisplayImagesWindow.xaml
    /// </summary>
    public partial class DisplayImagesWindow : Window
    {
        private string imagesFolderPath;
        private string jsonFilePath;
        private DisplayImagesViewModel viewModel;
        private int selectedImageIndex;
        private Guid selectedGuid;

        public DisplayImagesWindow(string imagesFolderPath, string jsonFilePath, int _selectedImageIndex, Guid _selectedGuid )
        {
            this.selectedImageIndex = _selectedImageIndex;
            this.selectedGuid = _selectedGuid;

            InitializeComponent();
            this.imagesFolderPath = imagesFolderPath;
            this.jsonFilePath = jsonFilePath;
            viewModel = new DisplayImagesViewModel(imagesFolderPath, jsonFilePath , selectedImageIndex , selectedGuid );
            this.DataContext = viewModel;
            

            ZonesList.SelectionChanged += viewModel.ListBox_SelectionChanged;
            ZonesList.MouseDoubleClick += viewModel.ZonesList_MouseDoubleClick;
        }

     
       

     
    }
}
