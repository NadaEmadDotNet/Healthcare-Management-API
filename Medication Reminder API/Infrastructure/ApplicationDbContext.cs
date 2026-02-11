using Medication_Reminder_API.Domain.Models;
using Medication_Reminder_API.Infrastructure;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Caregiver> Caregivers { get; set; }
    public DbSet<Medication> Medications { get; set; }
    public DbSet<DoseLog> DoseLogs { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<PatientCaregiver> PatientCaregivers { get; set; }
    public DbSet<PatientMedication> PatientMedications { get; set; }
    public DbSet<DoctorPatient> DoctorPatients { get; set; }


    //public DbSet<TreatmentPlan> TreatmentPlans { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ===================== PatientCaregiver Many-to-Many =====================
        builder.Entity<PatientCaregiver>()
            .HasKey(pc => new { pc.PatientID, pc.CaregiverID });

        builder.Entity<PatientCaregiver>()
            .HasOne(pc => pc.Patient)
            .WithMany(p => p.PatientCaregivers)
            .HasForeignKey(pc => pc.PatientID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PatientCaregiver>()
            .HasOne(pc => pc.Caregiver)
            .WithMany(c => c.PatientCaregivers)
            .HasForeignKey(pc => pc.CaregiverID)
            .OnDelete(DeleteBehavior.Restrict);

        // ===================== DoseLog FKs =====================
        builder.Entity<DoseLog>()
    .HasOne(dl => dl.Patient)
    .WithMany(p => p.DoseLogs)
    .HasForeignKey(dl => dl.PatientID)
    .OnDelete(DeleteBehavior.Restrict);

    
        builder.Entity<PatientMedication>()
            .HasKey(pm => new { pm.PatientID, pm.MedicationID });

        builder.Entity<PatientMedication>()
            .HasOne(pm => pm.Patient)
            .WithMany(p => p.PatientMedications)
            .HasForeignKey(pm => pm.PatientID);



        builder.Entity<PatientMedication>()
            .HasOne(pm => pm.Medication)
            .WithMany(m => m.PatientMedications)
            .HasForeignKey(pm => pm.MedicationID);
        builder.Entity<Patient>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.Entity<DoctorPatient>()
       .HasKey(dp => new { dp.DoctorId, dp.PatientId });

        builder.Entity<DoctorPatient>()
            .HasOne(dp => dp.Doctor)
            .WithMany(d => d.DoctorPatients)
            .HasForeignKey(dp => dp.DoctorId);

        builder.Entity<DoctorPatient>()
            .HasOne(dp => dp.Patient)
            .WithMany(p => p.DoctorPatients)
            .HasForeignKey(dp => dp.PatientId);
    }
}
