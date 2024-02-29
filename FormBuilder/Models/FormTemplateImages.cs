using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Models
{
    public class FormTemplateImages
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ImageBase64 { get; set; }
        public List<FormTemplateImageZone> Zones { get; set; }
    }

    public class FormTemplateImageZone
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double ActualWidth { get; set; }
        public double ActualHeight { get; set; }
        public string Regex { get; set; }
        public string WhiteList { get; set; }
        public bool IsDuplicated { get; set; }
        public bool IsAnchorPoint { get; set; }
        public ZoneFieldType Type { get; set; }
        public int FormTemplateImageId { get; set; }
        public int? FieldsId { get; set; }
    }
    public enum ZoneFieldType
    {
        SideText = 1,
        TextBelow = 2,
        FullText = 3,
        ChequeNumber = 4,
        Date = 5,
        HandWriting = 6,
        HandSignature = 7
    }
}
