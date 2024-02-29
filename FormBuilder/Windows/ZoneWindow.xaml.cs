using FormBuilder.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static FormBuilder.DisplayImagesViewModel;

namespace FormBuilder.Windows
{
    /// <summary>
    /// Interaction logic for DefaultSettingsWindow.xaml
    /// </summary>
    public partial class ZoneWindow : Window
    {
        private string ocrText;
        private string indexingField;
        private string valueRegexPattern;
        private string selectedType;

        public string IndexingField => indexingField;
        public string ValueRegexPattern => valueRegexPattern;
        public string SelectedType => selectedType;
        private List<string> dummyList = new List<string> { "Invoice Number", "Date", "Total" };

        public ZoneWindow(string ocrText, ImageData imageData)
        {
            InitializeComponent();
            this.ocrText = ocrText;
            ocrTextBox.Text = ocrText;

            // Set default selection
            typeComboBox.SelectedIndex = 0;

            if (imageData != null)
            {
                indexingFieldTextBox.Text = imageData.Rect.IndexingField;
                regexTextBox.Text = imageData.Rect.Regex;

                SetSelectedType(imageData.Rect.Type);
            }

            fieldComboBox.ItemsSource = dummyList;
        }

        private void SetSelectedType(string type)
        {
            foreach (ComboBoxItem item in typeComboBox.Items)
            {
                if (item.Tag.ToString() == type)
                {
                    typeComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            indexingField = indexingFieldTextBox.Text;
            ComboBoxItem selectedItem = (ComboBoxItem)typeComboBox.SelectedItem;
            selectedType = selectedItem.Content.ToString();
            GenerateValueRegexPattern();
            DialogResult = true;
        }
        private void GenerateValueRegexPattern()
        {
            string escapedIndexingField = Regex.Escape(indexingField);
            string valuePattern = @"(.*)";
            valueRegexPattern = $"{escapedIndexingField}\\s*{valuePattern}$";
            regexTextBox.Text = valueRegexPattern;
        }

        private void ocrTextBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            indexingField = textBox.SelectedText.Trim();
            indexingFieldTextBox.Text = indexingField;
            GenerateValueRegexPattern();
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Window.GetWindow(this).DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private async void fieldComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

    

        public class ZoneSettingsDTO
        {
            public string IndexingField { get; set; }
            public string RegexPattern { get; set; }
            public int Type { get; set; }
        }


    }
}
