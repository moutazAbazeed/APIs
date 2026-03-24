using System.Threading.Tasks;
using Xunit;
using HardwareInfoApis.Api.Services;
using HardwareInfoApis.Api.Services.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using HardwareInfoApis.Models.Shared;

namespace HardwareInfoApis.Tests
{
    public class FingerprintServiceTests
    {
        private readonly IFingerprintService _service;

        public FingerprintServiceTests()
        {
            _service = new FingerprintService(new NullLogger<FingerprintService>());
        }

        [Fact]
        public void IsValidFingerprint_ReturnsFalse_ForNullOrEmpty()
        {
            Assert.False(_service.IsValidFingerprint(null));
            Assert.False(_service.IsValidFingerprint(string.Empty));
            Assert.False(_service.IsValidFingerprint(" "));
        }

        [Theory]
        [InlineData("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")]
        [InlineData("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF")]
        public void IsValidFingerprint_ReturnsTrue_ForValidHex(string fp)
        {
            Assert.True(_service.IsValidFingerprint(fp));
        }

        [Fact]
        public async Task ComputeFingerprintAsync_ReturnsDeterministicHash()
        {
            var hw = new DeviceHardwareInfo
            {
                Processor = new ProcessorInfo { ProcessorId = "PID123", Model = "X" },
                Bios = new BiosInfo { SerialNumber = "B123" },
                Storage = new StorageInfo { PrimaryDiskSerial = "D123", TotalDiskBytes = 1024 },
                Memory = new MemoryInfo { TotalPhysicalBytes = 4096 }
            };

            var h1 = await _service.ComputeFingerprintAsync(hw);
            var h2 = await _service.ComputeFingerprintAsync(hw);

            Assert.False(string.IsNullOrEmpty(h1));
            Assert.Equal(h1, h2);
            Assert.Equal(64, h1.Length);
        }
    }
}
