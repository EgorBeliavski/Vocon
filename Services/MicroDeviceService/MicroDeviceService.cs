using System.Runtime.InteropServices;

using NAudio.CoreAudioApi;

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
        private async Task<bool> HasEnabledDeviceAsync()
        {
            var microphones = await GetMicrophonesAsync();
            return microphones.Any(m => m.IsEnabled);
        }

        
        private async Task<bool> IsAccessAllowedAsync()
        {
            try
            {
                using var capture = new Windows.Media.Capture.MediaCapture();
                await capture.InitializeAsync(new Windows.Media.Capture.MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Audio
                });
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }


        private bool IsEndpointMuted()
        {
            try
            {
                using var enumerator = new MMDeviceEnumerator();
                using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
                return device.AudioEndpointVolume.Mute;
            }
            catch (COMException)
            {
                
                return true;
            }
        }
        public async Task<bool> IsMicrophoneAvailableAsync()
        {
            if (!await HasEnabledDeviceAsync())
                return false;

            if (!await IsAccessAllowedAsync())
                return false;

            if (IsEndpointMuted())
                return false;

            return true;
        }
    }


}
