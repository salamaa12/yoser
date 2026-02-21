using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Yoser_API.Data.Models;

namespace Yoser_API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<MedicalProvider> MedicalProviders { get; set; }
        public DbSet<MedicationReminder> MedicationReminders { get; set; }
        public DbSet<PharmacyOrder> PharmacyOrders { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ضبط سعر الكشف
            builder.Entity<MedicalProvider>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // منع المسح المتسلسل في المواعيد عشان الداتابيز متضربش لو مريض اتمسح
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Provider)
                .WithMany()
                .HasForeignKey(a => a.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    
}
}
