namespace Yoser_API.DOTs
{
    public class MedicationRequestDto
    {
        public string MedName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public DateTime ReminderTime { get; set; }  // ممكن تخليها DateTime
    }
}
