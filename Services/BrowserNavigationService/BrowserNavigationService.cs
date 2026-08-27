using System.Diagnostics;
using System.Text.Json;
using Vocon.Services.SettingLanguageService;
namespace Vocon.Services.BrowserNavigationService
{

    public interface IBrowserNavigationService
    {
        Task<bool> TryNavigateAsync(string spokenPhrase);
        void AddSite(string keyword, string url);
        void AddSynonym(string existingKey, string synonym);
        void RemoveSite(string keyword);
        IReadOnlyDictionary<string, string> GetAllSites();
    }
    public class BrowserNavigationService : IBrowserNavigationService
    {
        private const string SitesPreferencesKey = "browser_navigation_sites";
        private const string SearchEnginePreferencesKey = "browser_navigation_search_engine";



        private static readonly Dictionary<string, string[]> TriggerWordsByLanguage = new()
        {
            ["ru"] = new[] { "открой", "открыть", "перейди на", "перейди в", "зайди на", "зайди в", "найди", "запусти" },
            ["en"] = new[] { "open", "go to", "navigate to", "launch", "search for", "find" },
            ["fr"] = new[] { "ouvre", "ouvrir", "va sur", "va à", "lance", "cherche", "recherche" },
            ["de"] = new[] { "öffne", "öffnen", "geh zu", "gehe zu", "starte", "suche", "suche nach" }
        };

        private static readonly Dictionary<string, string[]> TrailingPrepositionsByLanguage = new()
        {
            ["ru"] = new[] { "на ", "в ", "к " },
            ["en"] = new[] { "to ", "the " },
            ["fr"] = new[] { "sur ", "à ", "le ", "la ", "les " },
            ["de"] = new[] { "zu ", "nach ", "auf ", "die ", "der ", "das " }
        };
        private const string DefaultSearchEngineUrl = "https://www.google.com/search?q=";

        private readonly ISettingLanguageService _languageService;
        private Dictionary<string, SiteEntry> _sites;

        public BrowserNavigationService(ISettingLanguageService languageService)
        {
            _languageService = languageService;
            _sites = LoadSites();
        }

        private string CurrentLanguage => _languageService.SelectedLanguageCode ?? "en";

        private Dictionary<string, SiteEntry> LoadSites()
        {
            var json = Preferences.Get(SitesPreferencesKey, string.Empty);

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var stored = JsonSerializer.Deserialize<Dictionary<string, SiteEntry>>(json);
                    if (stored != null && stored.Count > 0)
                        return stored;
                }
                catch (JsonException)
                {
                }
            }

            return GetDefaultSites();
        }


        private static Dictionary<string, SiteEntry> GetDefaultSites()
        {
            SiteEntry Entry(string url, params string[] synonyms) =>
                new() { Url = url, Synonyms = synonyms.ToList() };

            return new Dictionary<string, SiteEntry>(StringComparer.OrdinalIgnoreCase)
            {
                ["youtube"] = Entry("https://www.youtube.com",
                    "ютуб", "youtube", "youtube.com"),

                ["gmail"] = Entry("https://mail.google.com",
                    "почта", "гугл почта", "gmail", "mail", "e-mail", "courrier", "boîte mail", "e-mails", "post", "e-mail eröffnen"),

                ["ozon"] = Entry("https://www.ozon.ru",
                    "озон", "маркетплейс", "ozon"),

                ["wildberries"] = Entry("https://www.wildberries.ru",
                    "вайлдберриз", "вб", "wildberries"),

                ["github"] = Entry("https://github.com",
                    "гитхаб", "github"),

                ["vk"] = Entry("https://vk.com",
                    "вконтакте", "вк", "vk"),

                ["telegram"] = Entry("https://web.telegram.org",
                    "телеграм", "telegram", "тг", "télégramme")
            };
        }

        private void SaveSites()
        {
            var json = JsonSerializer.Serialize(_sites);
            Preferences.Set(SitesPreferencesKey, json);
        }

        public IReadOnlyDictionary<string, string> GetAllSites() =>
            _sites.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Url);

        public void AddSite(string keyword, string url)
        {
            if (string.IsNullOrWhiteSpace(keyword) || string.IsNullOrWhiteSpace(url))
                return;

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            var key = keyword.Trim();

            if (_sites.TryGetValue(key, out var existing))
            {
                existing.Url = url;
            }
            else
            {
                _sites[key] = new SiteEntry { Url = url, Synonyms = new List<string> { key } };
            }

            SaveSites();
        }

        public void AddSynonym(string existingKey, string synonym)
        {
            if (_sites.TryGetValue(existingKey, out var entry) && !string.IsNullOrWhiteSpace(synonym))
            {
                if (!entry.Synonyms.Contains(synonym, StringComparer.OrdinalIgnoreCase))
                    entry.Synonyms.Add(synonym);

                SaveSites();
            }
        }

        public void RemoveSite(string keyword)
        {
            if (_sites.Remove(keyword))
                SaveSites();
        }

        public Task<bool> TryNavigateAsync(string spokenPhrase)
        {
            if (string.IsNullOrWhiteSpace(spokenPhrase))
                return Task.FromResult(false);

            var siteName = ExtractSiteName(spokenPhrase, CurrentLanguage);

            if (string.IsNullOrWhiteSpace(siteName))
                return Task.FromResult(false);

            var url = ResolveUrl(siteName, CurrentLanguage);
            return Task.FromResult(OpenUrl(url));
        }

        private string? ExtractSiteName(string phrase, string language)
        {
            var normalized = phrase.Trim().ToLowerInvariant();

            var triggers = TriggerWordsByLanguage.TryGetValue(language, out var t)
                ? t
                : TriggerWordsByLanguage["en"];

            var matchedTrigger = triggers
                .Where(trigger => normalized.Contains(trigger))
                .OrderByDescending(trigger => trigger.Length)
                .FirstOrDefault();

            if (matchedTrigger == null)
                return null;

            var index = normalized.IndexOf(matchedTrigger, StringComparison.Ordinal);
            var remainder = normalized[(index + matchedTrigger.Length)..].Trim();

            var prepositions = TrailingPrepositionsByLanguage.TryGetValue(language, out var p)
                ? p
                : Array.Empty<string>();

            remainder = remainder.TrimStart(prepositions);

            return string.IsNullOrWhiteSpace(remainder) ? null : remainder.Trim();
        }

        private string ResolveUrl(string siteName, string language)
        {
            if (_sites.TryGetValue(siteName, out var exactEntry))
                return exactEntry.Url;

            var bySynonym = _sites.Values.FirstOrDefault(entry =>
                entry.Synonyms.Any(syn =>
                    siteName.Contains(syn, StringComparison.OrdinalIgnoreCase) ||
                    syn.Contains(siteName, StringComparison.OrdinalIgnoreCase)));

            if (bySynonym != null)
                return bySynonym.Url;

            return DefaultSearchEngineUrl + Uri.EscapeDataString(siteName);
        }

        private bool OpenUrl(string url)
        {
            try
            {
                var psi = new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private class SiteEntry
        {
            public string Url { get; set; } = string.Empty;
            public List<string> Synonyms { get; set; } = new();
        }
    }

    internal static class StringExtensions
    {
        public static string TrimStart(this string source, params string[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                if (source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return source[prefix.Length..];
            }
            return source;
        }
    }
}