using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yoser_API.Data.Models // ضفنا كلمة namespace هنا
{
    public class PharmacyOrder
    {
        [Key]
        public int Id { get; set; }
        public int PatientId { get; set; }

        [Required]
        public byte[] PrescriptionData { get; set; } // مخزنة كـ Byte Array في الداتابيز
        public string? ImageContentType { get; set; }

        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("PatientId")]
        public virtual PatientProfile Patient { get; set; }
    }
}