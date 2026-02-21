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
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AppointmentController(AppDbContext context) => _context = context;

        // ================= 1. حجز موعد جديد =================
        [HttpPost("book")]
        public async Task<IActionResult> Book(BookAppointmentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    Message = "يجب أن تكون مريضاً لتتمكن من الحجز."
                });
            }

            if (dto.AppointmentDate < DateTime.UtcNow)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    Message = "لا يمكن الحجز في تاريخ قديم."
                });
            }

            var appointment = new Appointment
            {
                PatientId = patient.Id,
                ProviderId = dto.ProviderId,
                AppointmentDate = dto.AppointmentDate,
                Notes = dto.Notes,
                Status = "Pending"
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                Message = "تم إرسال طلب الحجز بنجاح",
                Data = new { appointment.Id, appointment.Status }
            });
        }

        // ================= 2. عرض مواعيدي (للمريض) =================
        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointments = await _context.Appointments
                .Include(a => a.Provider)
                    .ThenInclude(p => p.User)
                .Where(a => a.Patient.UserId == userId)
                .Select(a => new {
                    a.Id,
                    a.AppointmentDate,
                    a.Status,
                    ProviderName = a.Provider.User.FullName,
                    a.Provider.Specialty,
                    ProviderType = a.Provider.Type.ToString()
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                Message = "تم جلب المواعيد بنجاح",
                Data = appointments
            });
        }

        // ================= 3. تحديث حالة الموعد =================
        [HttpPut("update-status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Status = false,
                    Message = "الموعد غير موجود"
                });
            }

            appointment.Status = newStatus; // Approved, Rejected, Cancelled
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                Message = "تم تحديث حالة الموعد بنجاح",
                Data = new { appointment.Id, NewStatus = appointment.Status }
            });
        }
    }
}