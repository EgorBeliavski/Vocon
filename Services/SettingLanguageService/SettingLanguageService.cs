using System;
using System.Collections.Generic;
using System.Text;

namespace Vocon.Services.SettingLanguageService
{
    public interface ISettingLanguageService
    {
        string SelectedLanguageCode { get; set; }
    }

    public class SettingLanguageService : ISettingLanguageService
    {
        private const string LanguageKey = "note_language";

        public string SelectedLanguageCode
        {
            get => Preferences.Get(LanguageKey, "auto");
            set => Preferences.Set(LanguageKey, value);
        }
    }
}
