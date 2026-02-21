using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yoser_API.Data.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Completed
        public string Notes { get; set; }

        [ForeignKey("PatientId")]
        public virtual PatientProfile Patient { get; set; }
        [ForeignKey("ProviderId")]
        public virtual MedicalProvider Provider { get; set; }
    }
}