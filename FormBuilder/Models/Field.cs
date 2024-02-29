namespace FormBuilder.Models
{

    public class Field
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public bool Required { get; set; }
        public string? InputValue { get; set; }
        public string? Hint { get; set; }
        public string? ErrorMessage { get; set; }
        public bool Unique { get; set; } = false;
    }
}
