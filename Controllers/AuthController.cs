using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Yoser_API.Data;
using Yoser_API.Data.Models;
using Yoser_API.DTOs;

namespace Yoser_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtDuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;

            _jwtKey = configuration["JWT:Key"] ?? throw new Exception("JWT:Key is missing");
            _jwtIssuer = configuration["JWT:Issuer"] ?? throw new Exception("JWT:Issuer is missing");
            _jwtAudience = configuration["JWT:Audience"] ?? throw new Exception("JWT:Audience is missing");
            _jwtDuration = int.Parse(configuration["JWT:DurationInDays"] ?? "7");
        }

        // ================= 1. REGISTER =================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        if (await _userManager.FindByEmailAsync(dto.Email) != null)
                            return BadRequest(new ApiResponse<object> { Status = false, Message = "الإيميل مستخدم بالفعل" });

                        var user = new ApplicationUser
                        {
                            UserName = dto.Email,
                            Email = dto.Email,
                            FullName = dto.FullName,
                            Role = dto.Role // تأكدنا من استخدام المسمى الموحد Role
                        };

                        var result = await _userManager.CreateAsync(user, dto.Password);

                        if (!result.Succeeded)
                            return BadRequest(new ApiResponse<IEnumerable<IdentityError>> { Status = false, Message = "خطأ في إنشاء الحساب", Data = result.Errors });

                        await _userManager.AddToRoleAsync(user, dto.Role.ToString());

                        // إنشاء البروفايل بناءً على الدور
                        if (dto.Role == UserRole.Patient)
                        {
                            var profile = new PatientProfile
                            {
                                UserId = user.Id,
                                MedicalCondition = "None",
                                ChronicDiseases = "لم يتم الإضافة بعد",
                                EmergencyContact = "Not Provided",
                                Category = dto.PatientType ?? PatientCategory.Senior
                            };
                            _context.PatientProfiles.Add(profile);
                        }
                        else if (dto.Role == UserRole.Provider)
                        {
                            _context.MedicalProviders.Add(new MedicalProvider
                            {
                                UserId = user.Id,
                                Type = dto.ProviderType ?? ProviderType.Doctor,
                                Specialty = "General", // قيمة افتراضية عشان الداتابيز متضربش
                                Address = "Not Specified", // ده الحل للـ Error اللي ظهرلك
                                Bio = "New Provider",
                                Price = 0,
                                IsAvailable = true
                            });
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Ok(new ApiResponse<object> { Status = true, Message = "تم إنشاء الحساب والبروفايل بنجاح" });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, new ApiResponse<string> { Status = false, Message = "حدث خطأ أثناء تنفيذ المعاملة", Data = ex.Message });
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string> { Status = false, Message = "حدث خطأ في الخادم", Data = ex.Message });
            }
        }

        // ================= 2. LOGIN =================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized(new ApiResponse<object> { Status = false, Message = "الإيميل أو كلمة السر خطأ" });
            }

            var token = GenerateJwtToken(user);

            // تجميع البيانات المطلوبة في الـ Data كما طلبت
            var authResponse = new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsAuthenticated = true,
                ExpiresOn = DateTime.UtcNow.AddDays(_jwtDuration)
            };

            return Ok(new ApiResponse<AuthResponseDto>
            {
                Status = true,
                Message = "تم تسجيل الدخول بنجاح",
                Data = authResponse
            });
        }

        // ================= 3. GET USER BY ID =================
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound(new ApiResponse<object> { Status = false, Message = "المستخدم غير موجود" });

            object details = null;
            if (user.Role == UserRole.Patient)
                details = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == id);
            else
                details = await _context.MedicalProviders.FirstOrDefaultAsync(p => p.UserId == id);

            var userData = new
            {
                user.Id,
                user.FullName,
                user.Email,
                Role = user.Role.ToString(),
                Details = details
            };

            return Ok(new ApiResponse<object> { Status = true, Message = "تم جلب بيانات المستخدم", Data = userData });
        }

        // ================= 4. JWT MACHINE =================
        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("FullName", user.FullName ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwtDuration),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}