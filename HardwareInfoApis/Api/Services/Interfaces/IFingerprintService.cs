using HardwareInfoApis.Models.Shared;
using System.Threading.Tasks;

namespace HardwareInfoApis.Api.Services.Interfaces
{
    public interface IFingerprintService
    {
        bool IsValidFingerprint(string fingerprint);
        string NormalizeFingerprint(string fingerprint);
        Task<string> ComputeFingerprintAsync(DeviceHardwareInfo hardware);
    }
}
