using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vocon.Services.AutoStartService;
using Vocon.Services.HotKeyService;
using Vocon.Services.MicroDeviceService;
using Vocon.Services.SettingLanguageService;

namespace Vocon.ViewModels
{
    public class LanguageOption
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class MicroOptions
    {
        public string Id { get; set; } = string.Empty;
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
        private readonly AutoStartService _autostartService;
        private readonly IHotKeyService _hotKeyService;
        private readonly IHotKeySettingsService _hotKeySettingsService;
        private readonly IHotKeyRecorderService _hotKeyRecorderService;
        private readonly IMicrophoneSettingsService _microphoneSettingsService;

        private bool _isInitializingMicrophone; // чтобы не писать в Preferences во время первичной загрузки списка

        public SettingsPageViewModel(
            ISettingLanguageService settingsService,
            MicroDeviceService microdeviceservice,
            AutoStartService autoStartService,
            IHotKeyService hotKeyService,
            IHotKeySettingsService hotKeySettingsService,
            IHotKeyRecorderService hotKeyRecorderService,
            IMicrophoneSettingsService microphoneSettingsService)
        {
            _settingsService = settingsService;
            _microservice = microdeviceservice;
            _autostartService = autoStartService;
            _hotKeyService = hotKeyService;
            _hotKeySettingsService = hotKeySettingsService;
            _hotKeyRecorderService = hotKeyRecorderService;
            _microphoneSettingsService = microphoneSettingsService;

            var savedCode = _settingsService.SelectedLanguageCode;
            SelectedNoteLanguage = NoteLanguages.FirstOrDefault(l => l.Code == savedCode)
                                    ?? NoteLanguages.First();

            var savedKeys = _hotKeySettingsService.Load();
            HotkeyDisplay = FormatHotkey(savedKeys);
        }

        [RelayCommand]
        public Task InitializeAsync() => LoadMicrophonesAsync(_microservice.GetMicrophonesAsync());

        [RelayCommand]
        private Task RefreshMicrophones() => LoadMicrophonesAsync(_microservice.RefreshDevicesAsync());

        private async Task LoadMicrophonesAsync(Task<IReadOnlyList<Windows.Devices.Enumeration.DeviceInformation>> fetch)
        {
            var devices = await fetch;
            System.Diagnostics.Debug.WriteLine($"Found {devices.Count} microphones");

            var savedId = _microphoneSettingsService.Load();
            var previousId = SelectedMicrophone?.Id ?? savedId;

            _isInitializingMicrophone = true;

            AvailableMicrophones.Clear();
            foreach (var device in devices)
                AvailableMicrophones.Add(new MicroOptions { Id = device.Id, DisplayName = device.Name });

            SelectedMicrophone = AvailableMicrophones.FirstOrDefault(m => m.Id == previousId)
                                  ?? AvailableMicrophones.FirstOrDefault();

            _isInitializingMicrophone = false;
        }

        [ObservableProperty]
        private LanguageOption selectedNoteLanguage;

        partial void OnSelectedNoteLanguageChanged(LanguageOption value)
        {
            _settingsService.SelectedLanguageCode = value.Code;
        }

        [ObservableProperty]
        private MicroOptions selectedMicrophone;

        partial void OnSelectedMicrophoneChanged(MicroOptions value)
        {
         
            if (_isInitializingMicrophone) return;

            _microphoneSettingsService.Save(value?.Id);
        }

        [ObservableProperty]
        private bool launchOnStartup;

        partial void OnLaunchOnStartupChanged(bool value)
        {
            if (value)
                AutoStartService.CreateLabel();
            else
                AutoStartService.DeleteLabel();
        }

        // ---- Hotkey ----

        [ObservableProperty]
        private string hotkeyDisplay = "Alt + Space";

        [ObservableProperty]
        private string hotkeyConflictWarning = string.Empty;

        [ObservableProperty]
        private bool hasHotkeyConflict;

        [ObservableProperty]
        private bool isRecordingHotkey;

        public string HotkeyHint => "Some combinations (e.g. Alt+Tab, Win+L, Ctrl+Alt+Delete) are reserved by Windows and can't be used";

        [RelayCommand]
        private void StartHotkeyCapture()
        {
            HasHotkeyConflict = false;
            HotkeyConflictWarning = string.Empty;
            IsRecordingHotkey = true;

            var previousDisplay = HotkeyDisplay;
            HotkeyDisplay = "Press a combination...";

            try
            {
                _hotKeyRecorderService.StartRecording(
                    onCaptured: keys => OnHotkeyCaptured(keys),
                    onCancelled: () => OnHotkeyRecordingCancelled(previousDisplay));
            }
            catch (InvalidOperationException ex)
            {
                IsRecordingHotkey = false;
                HotkeyDisplay = previousDisplay;
                HotkeyConflictWarning = ex.Message;
                HasHotkeyConflict = true;
            }
        }

        private void OnHotkeyCaptured(uint[] keys)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsRecordingHotkey = false;

                try
                {
                    _hotKeyService.ChangeHotKey(keys);
                    _hotKeySettingsService.Save(keys);
                    HotkeyDisplay = FormatHotkey(keys);
                    HasHotkeyConflict = false;
                    HotkeyConflictWarning = string.Empty;
                }
                catch (InvalidOperationException ex)
                {
                    var savedKeys = _hotKeySettingsService.Load();
                    HotkeyDisplay = FormatHotkey(savedKeys);
                    HotkeyConflictWarning = ex.Message;
                    HasHotkeyConflict = true;
                }
            });
        }

        private void OnHotkeyRecordingCancelled(string previousDisplay)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsRecordingHotkey = false;
                HotkeyDisplay = previousDisplay;
            });
        }

        [RelayCommand]
        private void ResetHotkeyToDefault()
        {
            try
            {
                _hotKeyService.ResetToDefault();
                _hotKeySettingsService.Save(Vocon.Services.HotKeyService.HotKeyService.DefaultKeys);
                HotkeyDisplay = FormatHotkey(Vocon.Services.HotKeyService.HotKeyService.DefaultKeys);
                HasHotkeyConflict = false;
                HotkeyConflictWarning = string.Empty;
            }
            catch (InvalidOperationException ex)
            {
                HotkeyConflictWarning = ex.Message;
                HasHotkeyConflict = true;
            }
        }

        private static string FormatHotkey(IReadOnlyCollection<uint> keys)
        {
            if (keys == null || keys.Count == 0) return string.Empty;
            return string.Join(" + ", keys.Select(VkToString));
        }

        private static string VkToString(uint vk) => vk switch
        {
            0x12 or 0xA4 or 0xA5 => "Alt",
            0x11 or 0xA2 or 0xA3 => "Ctrl",
            0x10 or 0xA0 or 0xA1 => "Shift",
            0x5B or 0x5C => "Win",
            0x20 => "Space",
            0x0D => "Enter",
            0x09 => "Tab",
            0x1B => "Esc",
            0x2E => "Delete",
            0x08 => "Backspace",
            >= 0x70 and <= 0x7B => $"F{vk - 0x6F}",
            >= 0x41 and <= 0x5A => ((char)vk).ToString(),
            >= 0x30 and <= 0x39 => ((char)vk).ToString(),
            _ => $"0x{vk:X2}"
        };
    }
}