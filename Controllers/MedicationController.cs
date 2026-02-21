using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yoser_API.Data;
using Yoser_API.Data.Models;
using Yoser_API.DOTs;
using Yoser_API.DTOs; // تأكد أن المجلد اسمه DTOs وليس DOTs

namespace Yoser_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // يتطلب Token للوصول
    public class MedicationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MedicationController(AppDbContext context)
        {
            _context = context;
        }

        // ================= 1. إضافة دواء جديد =================
        [HttpPost("add")]
        public async Task<IActionResult> AddMedication(MedicationRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return BadRequest(new ApiResponse<object> { Status = false, Message = "لم يتم العثور على بروفايل مريض لهذا المستخدم." });

            var medication = new MedicationReminder
            {
                MedName = dto.MedName,
                Dosage = dto.Dosage,
                ReminderTime = dto.ReminderTime,
                PatientId = profile.Id,
                IsTaken = false
            };

            _context.MedicationReminders.Add(medication);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<MedicationReminder>
            {
                Status = true,
                Message = "تم إضافة موعد الدواء بنجاح",
                Data = medication
            });
        }

        // ================= 2. عرض أدوية المستخدم =================
        [HttpGet("my-medications")]
        public async Task<IActionResult> GetMyMedications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return NotFound(new ApiResponse<object> { Status = false, Message = "البروفايل غير موجود." });

            var medications = await _context.MedicationReminders
                .Where(m => m.PatientId == profile.Id)
                .OrderBy(m => m.ReminderTime)
                .ToListAsync();

            return Ok(new ApiResponse<List<MedicationReminder>>
            {
                Status = true,
                Message = "تم جلب قائمة الأدوية بنجاح",
                Data = medications
            });
        }

        // ================= 3. تحديث حالة الدواء (تم أخذه) =================
        [HttpPut("mark-taken/{id}")]
        public async Task<IActionResult> MarkAsTaken(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var medication = await _context.MedicationReminders
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id && m.Patient.UserId == userId);

            if (medication == null)
                return NotFound(new ApiResponse<object> { Status = false, Message = "الدواء غير موجود أو لا تملك صلاحية الوصول إليه." });

            medication.IsTaken = true;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                Message = "تم تسجيل أخذ الجرعة بنجاح",
                Data = new { medication.Id, medication.IsTaken }
            });
        }

        // ================= 4. تعديل بيانات دواء موجود =================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateMedication(int id, MedicationRequestDto updatedMed)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingMed = await _context.MedicationReminders
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id && m.Patient.UserId == userId);

            if (existingMed == null)
                return NotFound(new ApiResponse<object> { Status = false, Message = "الدواء غير موجود للتعديل." });

            existingMed.MedName = updatedMed.MedName;
            existingMed.Dosage = updatedMed.Dosage;
            existingMed.ReminderTime = updatedMed.ReminderTime;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<MedicationReminder>
            {
                Status = true,
                Message = "تم تحديث بيانات الدواء بنجاح",
                Data = existingMed
            });
        }

        // ================= 5. حذف دواء =================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMedication(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var medication = await _context.MedicationReminders
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id && m.Patient.UserId == userId);

            if (medication == null)
                return NotFound(new ApiResponse<object> { Status = false, Message = "لم يتم العثور على الدواء لحذفه." });

            _context.MedicationReminders.Remove(medication);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                Message = "تم حذف الدواء بنجاح"
            });
        }
    }
}