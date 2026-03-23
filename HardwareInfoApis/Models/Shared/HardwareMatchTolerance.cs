namespace HardwareInfoApis.Models.Shared
{
    /// <summary>
    /// Defines how strictly to compare hardware fingerprints
    /// </summary>
    public enum HardwareMatchTolerance
    {
        /// <summary>
        /// All critical hardware IDs must match exactly (CPU, BIOS, Disk)
        /// Best for: Licensing, anti-tamper, security
        /// </summary>
        Strict,

        /// <summary>
        /// Allow OS reinstall (ignore OS info), require at least BIOS or Disk match
        /// Best for: User accounts, device recognition after OS upgrade
        /// </summary>
        Moderate,

        /// <summary>
        /// Only require CPU match - tolerant of hardware upgrades/VMs
        /// Best for: Analytics, non-critical device grouping
        /// </summary>
        Lenient
    }
}