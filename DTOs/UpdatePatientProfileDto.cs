namespace Yoser_API.DTOs
{
    public class UpdatePatientProfileDto
    {
        public string MedicalCondition { get; set; } = string.Empty;
        public int Age { get; set; }
        public string EmergencyContact { get; set; } = string.Empty;
    }
}