using HardwareInfoApis.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HardwareInfoApis.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<License> Licenses { get; set; }
        // public DbSet<User> Users { get; set; } // If using user accounts

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Device configuration
            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasIndex(d => d.DeviceFingerprint).IsUnique();
                entity.HasIndex(d => d.BiosSerial);
                entity.HasIndex(d => d.DiskSerial);
                entity.HasIndex(d => new { d.UserId, d.IsActive });

                entity.Property(d => d.HardwareInfoJson)
                    .HasColumnType("nvarchar(max)");
            });

            // License configuration
            modelBuilder.Entity<License>(entity =>
            {
                entity.HasIndex(l => l.LicenseKey).IsUnique();
                entity.HasIndex(l => new { l.IsActive, l.ExpiresAt });

                entity.HasMany(l => l.Devices)
                    .WithOne(d => d.License)
                    .HasForeignKey(d => d.LicenseId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Seed initial data (optional)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed a default "unlimited" license for testing
            modelBuilder.Entity<License>().HasData(
                new License
                {
                    Id = 1,
                    LicenseKey = "DEMO-UNLIMITED-KEY",
                    LicenseName = "Demo License",
                    LicenseType = LicenseType.Trial,
                    MaxDevices = 999,
                    IssuedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddYears(1),
                    IssuedBy = "System"
                }
            );
        }
    }
}