namespace Yoser_API.DTOs
{
    public class UpdateProviderProfileDto
    {
        public string Specialty { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}