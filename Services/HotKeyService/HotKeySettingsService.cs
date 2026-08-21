namespace Vocon.Services.HotKeyService
{
    public interface IHotKeySettingsService
    {
        void Save(IReadOnlyCollection<uint> keys);
        IReadOnlyCollection<uint> Load();
    }

    public class HotKeySettingsService : IHotKeySettingsService
    {
        private const string KeysPrefKey = "HotKeyKeys";

        public void Save(IReadOnlyCollection<uint> keys)
        {
            var serialized = string.Join(",", keys);
            Preferences.Set(KeysPrefKey, serialized);
        }

        public IReadOnlyCollection<uint> Load()
        {
            var defaultSerialized = string.Join(",", HotKeyService.DefaultKeys);
            var raw = Preferences.Get(KeysPrefKey, defaultSerialized);

            if (string.IsNullOrWhiteSpace(raw))
            {
                return HotKeyService.DefaultKeys;
            }

            var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var result = new List<uint>();

            foreach (var part in parts)
            {
                if (uint.TryParse(part, out var value))
                {
                    result.Add(value);
                }
            }

            return result.Count > 0 ? result : HotKeyService.DefaultKeys;
        }
    }
}