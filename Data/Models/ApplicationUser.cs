using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Yoser_API.Data.Models
{
        public class ApplicationUser : IdentityUser
        {
            [Required, MaxLength(100)]
            public string FullName { get; set; }
            public UserRole Role { get; set; } // هل هو مريض ولا مقدم خدمة؟
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
    

}
