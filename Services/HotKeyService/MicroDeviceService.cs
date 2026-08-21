namespace Vocon.Services.MicroDeviceService
{
    public interface IMicrophoneSettingsService
    {
        void Save(string? deviceId);
        string? Load();
    }

    public class MicrophoneSettingsService : IMicrophoneSettingsService
    {
        private const string DeviceIdPrefKey = "SelectedMicrophoneId";

        public void Save(string? deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                Preferences.Remove(DeviceIdPrefKey);
                return;
            }

            Preferences.Set(DeviceIdPrefKey, deviceId);
        }

        public string? Load()
        {
            return Preferences.Get(DeviceIdPrefKey, (string?)null);
        }
    }
}