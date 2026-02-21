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
    public class ProviderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProviderController(AppDbContext context)
        {
            _context = context;
        }

        // ================= 1. تحديث بيانات مقدم الخدمة (الطبيب/الممرض) =================
        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProviderProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var provider = await _context.MedicalProviders.FirstOrDefaultAsync(p => p.UserId == userId);

            if (provider == null)
            {
                // في حالة لم يتم إنشاؤه أثناء الـ Register لسبب ما
                provider = new MedicalProvider { UserId = userId };
                _context.MedicalProviders.Add(provider);
            }

            provider.Specialty = dto.Specialty;
            provider.Bio = dto.Bio;
            provider.Address = dto.Address;
            provider.Price = dto.Price;
            provider.IsAvailable = dto.IsAvailable;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                Message = "تم تحديث بياناتك المهنية بنجاح.",
                Data = new { provider.Id, provider.Specialty, provider.IsAvailable }
            });
        }

        // ================= 2. الحصول على بيانات بروفايلي (للطبيب نفسه) =================
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var provider = await _context.MedicalProviders
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (provider == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Status = false,
                    Message = "لم يتم العثور على بيانات مقدم الخدمة."
                });
            }

            var providerData = new
            {
                provider.Id,
                provider.User.FullName,
                provider.User.Email,
                provider.Specialty,
                provider.Bio,
                provider.Address,
                provider.Price,
                provider.IsAvailable,
                Type = provider.Type.ToString()
            };

            return Ok(new ApiResponse<object>
            {
                Status = true,
                Message = "تم جلب بيانات البروفايل بنجاح",
                Data = providerData
            });
        }

        // ================= 3. عرض مقدمي الخدمة المتاحين (للمرضى) =================
        [HttpGet("all-providers")]
        [AllowAnonymous] // متاح للبحث حتى قبل تسجيل الدخول
        public async Task<IActionResult> GetAllProviders([FromQuery] string? specialty)
        {
            var query = _context.MedicalProviders
                .Include(p => p.User)
                .Where(p => p.IsAvailable);

            if (!string.IsNullOrEmpty(specialty))
            {
                query = query.Where(p => p.Specialty.Contains(specialty));
            }

            var providers = await query.Select(p => new {
                p.Id,
                p.User.FullName,
                p.Specialty,
                p.Price,
                p.Address,
                p.Bio,
                Type = p.Type.ToString()
            }).ToListAsync();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                Message = $"تم العثور على ({providers.Count}) مقدم خدمة.",
                Data = providers
            });
        }
    }
}