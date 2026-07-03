using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Vocon.Services.SettingLanguageService;

namespace Vocon.ViewModels
{
    public class LanguageOption
    {
        public string DisplayName { get; set; }
        public string Code { get; set; }
    }
    public partial class SettingsPageViewModel : ObservableObject
    {
        public ObservableCollection<LanguageOption> NoteLanguages { get; } = new()
        {
            new() { DisplayName = "Auto", Code = "auto" },
            new() { DisplayName = "Русский", Code = "ru" },
            new() { DisplayName = "English", Code = "en" },
            new() { DisplayName = "Français", Code = "fr" },
            new() { DisplayName = "Deutsch", Code = "de" }
        };
        private readonly ISettingLanguageService _settingsService;

        public SettingsPageViewModel(ISettingLanguageService settingsService)
        {
            _settingsService = settingsService;

            var savedCode = _settingsService.SelectedLanguageCode;
            SelectedNoteLanguage = NoteLanguages.FirstOrDefault(l => l.Code == savedCode)
                                    ?? NoteLanguages.First();
        }

        [ObservableProperty]
        private LanguageOption selectedNoteLanguage;

        partial void OnSelectedNoteLanguageChanged(LanguageOption value)
        {
            _settingsService.SelectedLanguageCode = value.Code;
        }
    }
}
