using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yoser_API.Data;
using Yoser_API.Data.Models;
using Yoser_API.DOTs;
using Yoser_API.DTOs;

namespace Yoser_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PharmacyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PharmacyController(AppDbContext context)
        {
            _context = context;
        }

        // ================= 1. رفع طلب صيدلية جديد =================
        [HttpPost("upload-prescription")]
        public async Task<IActionResult> UploadPrescription([FromForm] CreateOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    Message = "بروفايل المريض غير موجود."
                });
            }

            if (dto.ImageFile == null || dto.ImageFile.Length == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    Message = "يرجى رفع صورة الروشتة."
                });
            }

            // تحويل الصورة إلى Byte Array
            using var memoryStream = new MemoryStream();
            await dto.ImageFile.CopyToAsync(memoryStream);

            var order = new PharmacyOrder
            {
                PatientId = patient.Id,
                PrescriptionData = memoryStream.ToArray(),
                ImageContentType = dto.ImageFile.ContentType,
                Status = "Pending"
            };

            _context.PharmacyOrders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                Message = "تم رفع الطلب بنجاح وتخزين الصورة.",
                Data = new { OrderId = order.Id, order.Status }
            });
        }

        // ================= 2. عرض الصورة (مستثنى من Wrapper) =================
        [HttpGet("image/{orderId}")]
        [AllowAnonymous] // للسماح للمتصفح أو الموبايل بعرض الصورة مباشرة عبر الرابط
        public async Task<IActionResult> GetPrescriptionImage(int orderId)
        {
            var order = await _context.PharmacyOrders.FindAsync(orderId);

            if (order == null || order.PrescriptionData == null)
                return NotFound();

            // هنا نرجع File مباشرة وليس ApiResponse لأن هذا Endpoint لغرض العرض فقط
            return File(order.PrescriptionData, order.ImageContentType ?? "image/jpeg");
        }

        // ================= 3. عرض جميع طلباتي كمريض =================
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.PharmacyOrders
                .Where(o => o.Patient.UserId == userId)
                .Select(o => new {
                    o.Id,
                    o.Status,
                    // إنشاء رابط مباشر للصورة ليسهل على الفرونت إند عرضها
                    ImageUrl = $"{Request.Scheme}://{Request.Host}/api/Pharmacy/image/{o.Id}"
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                Message = "تم جلب الطلبات بنجاح",
                Data = orders
            });
        }
    }
}