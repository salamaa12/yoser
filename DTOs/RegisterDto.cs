using System.ComponentModel.DataAnnotations;
using Yoser_API.Data.Models; // عشان يشوف الـ UserType Enum

namespace Yoser_API.DTOs
{
    public class RegisterDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; } // Patient or Provider
                                           // لو مريض حدد (Senior أو Determination)
        public PatientCategory? PatientType { get; set; }
        // لو مقدم خدمة حدد (Doctor أو Nurse)
        public ProviderType? ProviderType { get; set; }
    }
}
