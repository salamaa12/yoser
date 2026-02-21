using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yoser_API.Data.Models
{
    public class MedicalProvider
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }

        [Required]
        public ProviderType Type { get; set; } // دكتور أو ممرض

        [Required, MaxLength(100)]
        public string Specialty { get; set; }
        public string Bio { get; set; }
        public string Address { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}