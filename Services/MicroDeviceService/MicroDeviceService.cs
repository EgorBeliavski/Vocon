using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Windows.Devices.Enumeration;

namespace Vocon.Services.MicroDeviceService
{
    public class MicroDeviceService
    {
        public async Task<IReadOnlyList<DeviceInformation>> GetMicrophonesAsync()
        {
            var microphones = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);
            return microphones;
        }

        public Task<IReadOnlyList<DeviceInformation>> RefreshDevicesAsync()
            => GetMicrophonesAsync();
    }


}
