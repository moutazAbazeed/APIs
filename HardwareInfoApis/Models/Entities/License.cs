using HardwareInfoApis.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HardwareInfoApis.Api.Models.Entities
{
    [Table("Licenses")]
    public class License
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Key]
        public string LicenseKey { get; set; } = string.Empty;

        [MaxLength(255)]
        public string LicenseName { get; set; } = "Standard License";

        [Required]
        public LicenseType LicenseType { get; set; } = LicenseType.Standard;

        public int MaxDevices { get; set; } = 1;

        [Required]
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? ActivatedAt { get; set; }

        public bool IsActive =>
            !IsRevoked && (!ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow);

        public bool IsRevoked { get; set; }
        public string? RevokedReason { get; set; }
        public DateTime? RevokedAt { get; set; }

        // Usage tracking
        public int CurrentDeviceCount { get; set; }

        // Metadata
        [MaxLength(100)]
        public string? IssuedBy { get; set; }
        [MaxLength(255)]
        public string? CustomerEmail { get; set; }
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
    }

    public enum LicenseType
    {
        Trial = 0,
        Standard = 1,
        Professional = 2,
        Enterprise = 3,
        Lifetime = 4
    }
}