using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace HardwareInfoApis.Api.Models.Entities
{
    [Table("Devices")]
    public class Device
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string DeviceFingerprint { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string DeviceName { get; set; } = string.Empty;

        // Hardware info (stored as JSON for flexibility)
        [Column(TypeName = "nvarchar(max)")]
        public string HardwareInfoJson { get; set; } = string.Empty;

        // Processor info (indexed for search)
        [MaxLength(100)]
        public string ProcessorId { get; set; }
        [MaxLength(255)]
        public string ProcessorModel { get; set; }

        // Memory info
        [MaxLength(50)]
        public string TotalRam { get; set; }
        public long? TotalRamBytes { get; set; }

        // BIOS info
        [MaxLength(100)]
        public string BiosSerial { get; set; }

        // Storage info
        [MaxLength(100)]
        public string DiskSerial { get; set; }
        [MaxLength(255)]
        public string DiskModel { get; set; }

        // OS info
        [MaxLength(255)]
        public string OsName { get; set; }
        [MaxLength(50)]
        public string OsBuild { get; set; }

        // Registration metadata
        [Required]
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastSeenAt { get; set; }
        public string? LastIpAddress { get; set; }
        public string? LastAppVersion { get; set; }

        // Optional: Link to user account
        public Guid? UserId { get; set; }

        // Optional: Link to license
        public int? LicenseId { get; set; }

        // Status flags
        public bool IsActive { get; set; } = true;
        public bool IsBlocked { get; set; } = false;
        public string? BlockReason { get; set; }

        // Navigation properties
        public virtual License? License { get; set; }
        // public virtual User? User { get; set; } // If using user accounts

        // Computed properties (not mapped)
        [NotMapped]
        public bool RequiresUpdate =>
            LastSeenAt.HasValue && LastSeenAt.Value < DateTime.UtcNow.AddDays(-90);
    }
}