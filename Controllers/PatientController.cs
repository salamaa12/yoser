using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yoser_API.Data;
using Yoser_API.Data.Models;
using Yoser_API.DTOs;

namespace Yoser_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // يتطلب تسجيل دخول
    public class PatientController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PatientController(AppDbContext context)
        {
            _context = context;
        }

        // ================= 1. الحصول على بيانات البروفايل الخاص بي =================
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            // جلب الـ ID الخاص بالمستخدم من التوكن
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // جلب البروفايل مع بيانات اليوزر الأساسية (FullName, Email)
            var profile = await _context.PatientProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Status = false,
                    Message = "لم يتم العثور على بروفايل لهذا المستخدم."
                });
            }

            // عرض البيانات بشكل منظم داخل الـ Data
            var profileData = new
            {
                profile.Id,
                profile.User.FullName,
                profile.User.Email,
                profile.Age,
                profile.MedicalCondition, // ضفناه عشان التناسق
                profile.ChronicDiseases,
                profile.EmergencyContact,
                Category = profile.Category.ToString(),
                JoinedAt = profile.User.CreatedAt
            };

            return Ok(new ApiResponse<object>
            {
                Status = true,
                Message = "تم جلب بيانات البروفايل بنجاح",
                Data = profileData
            });
        }

        // ================= 2. تحديث بيانات البروفايل =================
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdatePatientProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    Message = "البروفايل غير موجود."
                });
            }

            // تحديث الحقول
            profile.Age = dto.Age;
            profile.ChronicDiseases = dto.MedicalCondition; // هنا استعملنا MedicalCondition من الـ DTO لملء ChronicDiseases
            profile.EmergencyContact = dto.EmergencyContact;

            _context.PatientProfiles.Update(profile);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                Message = "تم تحديث البيانات الطبية بنجاح.",
                Data = new { profile.Id, profile.Age, profile.ChronicDiseases }
            });
        }
    }
}