namespace HardwareInfoApis.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HardwareInfoApis.Api.Models.Entities;
    using HardwareInfoApis.Models.Api.Responses;

    /// <summary>
    /// Service interface for license management operations
    /// Handles license validation, activation, expiration, and revocation
    /// </summary>
    public interface ILicenseService
    {
        #region License Validation

        /// <summary>
        /// Validate a license key
        /// </summary>
        /// <param name="licenseKey">License key to validate</param>
        /// <param name="deviceFingerprint">Device fingerprint requesting validation</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>License validation result with status and details</returns>
        Task<LicenseValidationResult> ValidateLicenseAsync(
            string licenseKey,
            string? deviceFingerprint = null,
            CancellationToken ct = default);

        /// <summary>
        /// Check if a license is active and valid
        /// </summary>
        /// <param name="licenseKey">License key to check</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if license is active and valid</returns>
        Task<bool> IsLicenseActiveAsync(
            string licenseKey,
            CancellationToken ct = default);

        #endregion

        #region License Management

        /// <summary>
        /// Create a new license key
        /// </summary>
        /// <param name="licenseType">Type of license to create</param>
        /// <param name="expiryDays">Number of days until expiration (null for lifetime)</param>
        /// <param name="maxDevices">Maximum devices allowed (0 for unlimited)</param>
        /// <param name="customerEmail">Customer email for notifications</param>
        /// <param name="issuedBy">Admin/user who issued the license</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Created license with key</returns>
        Task<License> CreateLicenseAsync(
            LicenseType licenseType,
            int? expiryDays = null,
            int maxDevices = 1,
            string? customerEmail = null,
            string? issuedBy = null,
            CancellationToken ct = default);

        /// <summary>
        /// Activate a license on a device
        /// </summary>
        /// <param name="licenseKey">License key to activate</param>
        /// <param name="deviceFingerprint">Device fingerprint to bind</param>
        /// <param name="deviceId">Device database ID</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Activation result</returns>
        Task<LicenseActivationResult> ActivateLicenseAsync(
            string licenseKey,
            string deviceFingerprint,
            int deviceId,
            CancellationToken ct = default);

        /// <summary>
        /// Deactivate a license from a device
        /// </summary>
        /// <param name="licenseKey">License key</param>
        /// <param name="deviceFingerprint">Device fingerprint to remove</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if successfully deactivated</returns>
        Task<bool> DeactivateLicenseAsync(
            string licenseKey,
            string deviceFingerprint,
            CancellationToken ct = default);

        /// <summary>
        /// Revoke a license (invalidate completely)
        /// </summary>
        /// <param name="licenseKey">License key to revoke</param>
        /// <param name="reason">Reason for revocation</param>
        /// <param name="revokedBy">Admin who revoked</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if successfully revoked</returns>
        Task<bool> RevokeLicenseAsync(
            string licenseKey,
            string reason,
            string? revokedBy = null,
            CancellationToken ct = default);

        /// <summary>
        /// Extend license expiration date
        /// </summary>
        /// <param name="licenseKey">License key to extend</param>
        /// <param name="additionalDays">Number of days to add</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Updated license with new expiry date</returns>
        Task<License> ExtendLicenseAsync(
            string licenseKey,
            int additionalDays,
            CancellationToken ct = default);

        #endregion

        #region License Queries

        /// <summary>
        /// Get license by key
        /// </summary>
        /// <param name="licenseKey">License key to lookup</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>License entity or null if not found</returns>
        Task<License?> GetLicenseByKeyAsync(
            string licenseKey,
            CancellationToken ct = default);

        /// <summary>
        /// Get license by device fingerprint
        /// </summary>
        /// <param name="deviceFingerprint">Device fingerprint</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>License entity or null if not found</returns>
        Task<License?> GetLicenseByDeviceAsync(
            string deviceFingerprint,
            CancellationToken ct = default);

        /// <summary>
        /// Get all licenses for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of licenses</returns>
        Task<List<License>> GetUserLicensesAsync(
            string userId,
            CancellationToken ct = default);

        /// <summary>
        /// Get all active licenses
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of active licenses</returns>
        Task<List<License>> GetActiveLicensesAsync(
            CancellationToken ct = default);

        /// <summary>
        /// Get licenses expiring soon
        /// </summary>
        /// <param name="daysThreshold">Number of days to check (e.g., 30 for expiring within 30 days)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of licenses expiring soon</returns>
        Task<List<License>> GetLicensesExpiringSoonAsync(
            int daysThreshold = 30,
            CancellationToken ct = default);

        #endregion

        #region License Statistics

        /// <summary>
        /// Get license usage statistics
        /// </summary>
        /// <param name="licenseKey">License key</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>License usage statistics</returns>
        Task<LicenseStatistics> GetLicenseStatisticsAsync(
            string licenseKey,
            CancellationToken ct = default);

        /// <summary>
        /// Get total license counts by type
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Dictionary of license type counts</returns>
        Task<Dictionary<LicenseType, int>> GetLicenseCountsByTypeAsync(
            CancellationToken ct = default);

        #endregion

        #region License Transfer

        /// <summary>
        /// Transfer license from one device to another
        /// </summary>
        /// <param name="licenseKey">License key to transfer</param>
        /// <param name="fromDeviceFingerprint">Source device fingerprint</param>
        /// <param name="toDeviceFingerprint">Destination device fingerprint</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Transfer result</returns>
        Task<LicenseTransferResult> TransferLicenseAsync(
            string licenseKey,
            string fromDeviceFingerprint,
            string toDeviceFingerprint,
            CancellationToken ct = default);

        #endregion

        #region License Key Generation

        /// <summary>
        /// Generate a new unique license key
        /// </summary>
        /// <param name="licenseType">Type of license</param>
        /// <returns>Generated license key string</returns>
        string GenerateLicenseKey(LicenseType licenseType);

        /// <summary>
        /// Validate license key format
        /// </summary>
        /// <param name="licenseKey">License key to validate</param>
        /// <returns>True if format is valid</returns>
        bool IsValidLicenseKeyFormat(string licenseKey);

        #endregion
    }
}