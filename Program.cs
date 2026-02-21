using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using Yoser_API.Data;
using Yoser_API.Data.Models;
var builder = WebApplication.CreateBuilder(args);

// ================= 1. CORS Configuration =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ================= 2. Database Context =================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

// ================= 3. Identity Configuration (Modified) =================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // الإعدادات السهلة اللي طلبتها
    options.Password.RequireDigit = false;             // لا يشترط أرقام
    options.Password.RequiredLength = 6;                // الطول الأدنى 6 خانات فقط
    options.Password.RequireNonAlphanumeric = false;    // لا يشترط رموز (@, #, !)
    options.Password.RequireUppercase = false;          // لا يشترط حروف كبيرة
    options.Password.RequireLowercase = false;          // لا يشترط حروف صغيرة
    options.Password.RequiredUniqueChars = 1;           // حرف واحد مختلف يكفي

    options.User.RequireUniqueEmail = true;             // الحفاظ على فريد الإيميل (أمان أساسي)
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ================= 4. Authentication (JWT) - التعديل الجذري هنا =================
var jwtSettings = builder.Configuration.GetSection("JWT");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],

        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// ================= 5. Middleware Order (الترتيب حياة أو موت هنا) =================

app.UseCors("AllowAll");

// تعطيل الـ HTTPS Redirection مؤقتاً لحل مشاكل الـ Mixed Content في الاستضافات المجانية
// app.UseHttpsRedirection(); 

app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthentication(); // يجب أن يسبق الـ Authorization
app.UseAuthorization();

// ================= 6. Safe Role Seeding =================

using (var scope = app.Services.CreateScope())
{
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // غيرنا UserType لـ UserRole عشان يطابق الـ Models الجديدة
        var roles = Enum.GetNames<UserRole>();

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("ROLE SEED ERROR: " + ex.Message);
    }
}

app.MapControllers();
app.Run();