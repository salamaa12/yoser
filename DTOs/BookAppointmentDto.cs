namespace Yoser_API.DTOs
{
    public class BookAppointmentDto
    {
        public int ProviderId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Notes { get; set; }
    }
}