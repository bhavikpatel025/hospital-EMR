using EMR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<PatientDocument> PatientDocuments => Set<PatientDocument>();
    public DbSet<PatientMedication> PatientMedications => Set<PatientMedication>();
    public DbSet<PatientLabFinding> PatientLabFindings => Set<PatientLabFinding>();
    public DbSet<PatientRadiologyNote> PatientRadiologyNotes => Set<PatientRadiologyNote>();
    public DbSet<JointAssessment> JointAssessments => Set<JointAssessment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("Patients");
            entity.HasIndex(p => p.Mobile);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.ToTable("Doctors");

            entity.Property(d => d.ConsultationFee)
          .HasPrecision(10, 2);

            entity.HasOne(d => d.User)
                  .WithOne()
                  .HasForeignKey<Doctor>(d => d.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");

            entity.HasOne(a => a.Patient)
                  .WithMany()
                  .HasForeignKey(a => a.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Doctor)
                  .WithMany()
                  .HasForeignKey(a => a.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => a.AppointmentDate);
            entity.Property(a => a.Status).HasConversion<string>();   // Enum ko DB mein readable string store karega
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasIndex(r => r.RoleName).IsUnique();
        });

        // Seed initial Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "Doctor" },
            new Role { RoleId = 3, RoleName = "Receptionist" }
        );

        modelBuilder.Entity<PatientDocument>(entity =>
        {
            entity.ToTable("PatientDocuments");
            entity.HasOne(d => d.Patient).WithMany().HasForeignKey(d => d.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PatientMedication>(entity =>
        {
            entity.ToTable("PatientMedications");
            entity.HasOne(m => m.Patient).WithMany().HasForeignKey(m => m.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PatientLabFinding>(entity =>
        {
            entity.ToTable("PatientLabFindings");
            entity.HasOne(l => l.Patient).WithMany().HasForeignKey(l => l.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PatientRadiologyNote>(entity =>
        {
            entity.ToTable("PatientRadiologyNotes");
            entity.HasOne(r => r.Patient).WithMany().HasForeignKey(r => r.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JointAssessment>(entity =>
        {
            entity.ToTable("JointAssessments");
            entity.HasOne(a => a.Patient).WithMany().HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}