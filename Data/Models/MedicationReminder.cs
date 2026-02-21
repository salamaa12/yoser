using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yoser_API.Data.Models
{
    public class MedicationReminder
    {
        [Key]
        public int Id { get; set; }
        public int PatientId { get; set; }

        [Required, MaxLength(100)]
        public string MedName { get; set; }
        public string Dosage { get; set; }
        public DateTime ReminderTime { get; set; }
        public bool IsTaken { get; set; } = false;

        [ForeignKey("PatientId")]
        public virtual PatientProfile Patient { get; set; }
    }

}
