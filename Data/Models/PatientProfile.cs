using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yoser_API.Data.Models
{
    public class PatientProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public PatientCategory Category { get; set; } // كبير سن أو ذوي همم

        public int Age { get; set; }

        [Required]
        public string EmergencyContact { get; set; }

        // الحقل المطلوب لحل خطأ الـ AuthController
        public string MedicalCondition { get; set; } = "None";

        // لإضافة الأمراض (اللي اتفقنا عليها في المخطط)
        public string ChronicDiseases { get; set; } = "None";

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}