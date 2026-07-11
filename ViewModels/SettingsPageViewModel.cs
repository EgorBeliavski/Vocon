using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vocon.Services.HotKeyService;
using Vocon.Services.MicroDeviceService;
using Vocon.Services.SettingLanguageService;
using Windows.System.Preview;

namespace Vocon.ViewModels
{
    public class LanguageOption
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class MicroOptions
    {
        public string Id{  get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }


    public partial class SettingsPageViewModel : ObservableObject
    {
        public ObservableCollection<MicroOptions> AvailableMicrophones { get; } = new();

        public ObservableCollection<LanguageOption> NoteLanguages { get; } = new()
        {
            new() { DisplayName = "Auto", Code = "auto" },
            new() { DisplayName = "Русский", Code = "ru" },
            new() { DisplayName = "English", Code = "en" },
            new() { DisplayName = "Français", Code = "fr" },
            new() { DisplayName = "Deutsch", Code = "de" }
        };
        private readonly ISettingLanguageService _settingsService;
        private readonly MicroDeviceService _microservice;
        public SettingsPageViewModel(ISettingLanguageService settingsService,MicroDeviceService microdeviceservice)
        {
            _settingsService = settingsService;
            _microservice = microdeviceservice;
            var savedCode = _settingsService.SelectedLanguageCode;
            SelectedNoteLanguage = NoteLanguages.FirstOrDefault(l => l.Code == savedCode)
                                    ?? NoteLanguages.First();
        }

        [RelayCommand]
        public Task InitializeAsync() => LoadMicrophonesAsync(_microservice.GetMicrophonesAsync());

        [RelayCommand]
        private Task RefreshMicrophones() => LoadMicrophonesAsync(_microservice.RefreshDevicesAsync());


        private async Task LoadMicrophonesAsync(Task<IReadOnlyList<Windows.Devices.Enumeration.DeviceInformation>> fetch)
        {
            var devices = await fetch;
            System.Diagnostics.Debug.WriteLine($"Found {devices.Count} microphones");
            var previousId = SelectedMicrophone?.Id;

            AvailableMicrophones.Clear();
            foreach (var device in devices)
                AvailableMicrophones.Add(new MicroOptions { Id = device.Id, DisplayName = device.Name });

            SelectedMicrophone = AvailableMicrophones.FirstOrDefault(m => m.Id == previousId)
                                  ?? AvailableMicrophones.FirstOrDefault();
        }


        


        [ObservableProperty]
        private LanguageOption selectedNoteLanguage;

        partial void OnSelectedNoteLanguageChanged(LanguageOption value)
        {
            _settingsService.SelectedLanguageCode = value.Code;
        }


        [ObservableProperty]
        private MicroOptions selectedMicrophone;
    }
}
