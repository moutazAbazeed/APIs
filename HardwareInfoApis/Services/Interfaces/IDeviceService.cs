using HardwareInfoApis.Api.Models.Api.Responses;
using HardwareInfoApis.Api.Models.Entities;
using HardwareInfoApis.Models.Api.Requests;
using HardwareInfoApis.Models.Api.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareInfoApis.Api.Services.Interfaces
{
    public interface IDeviceService
    {
        Task<ApiResponse<CheckDeviceResponse>> CheckDeviceAsync(
            CheckDeviceRequest request, string? clientIp, CancellationToken ct = default);

        Task<ApiResponse<RegisterDeviceResponse>> RegisterDeviceAsync(
            RegisterDeviceRequest request, string? clientIp, CancellationToken ct = default);

        Task<Device?> GetDeviceByFingerprintAsync(string fingerprint, CancellationToken ct = default);
        Task<bool> UpdateDeviceLastSeenAsync(int deviceId, string clientIp, string appVersion, CancellationToken ct = default);
    }
}