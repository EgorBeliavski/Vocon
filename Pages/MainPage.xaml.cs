using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Audio;
using System.Diagnostics;
using Vocon.Services.EmbeddingServices;
using Vocon.ViewModels;
using Vocon.Platforms.Windows;
using Whisper.net;

namespace Vocon
{
    public partial class MainPage : ContentPage
    {
        readonly WindowChromeService? _chrome;
        bool _notesLoaded;


        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            Loaded += async (s, e) =>
            {
                if (_notesLoaded) return;
                _notesLoaded = true;
                await viewModel.LoadNotesAsync();
            };

#if WINDOWS
            _chrome = IPlatformApplication.Current?.Services.GetService<WindowChromeService>();
#endif
        }

        void OnDragZonePressed(object? sender, PointerEventArgs e) => _chrome?.StartDrag();

        void OnMinimizeClicked(object? sender, EventArgs e) => _chrome?.Minimize();

        void OnMaximizeClicked(object? sender, EventArgs e)
        {
            _chrome?.ToggleMaximize();
            if (_chrome is not null)
            {
                MaximizeButton.ImageSource = _chrome.IsMaximized
                    ? "icon_restore_lostmedia.png"
                    : "icon_maximize_lostmedia.png";
            }
        }

        void OnCloseClicked(object? sender, EventArgs e) => _chrome?.Close();
    }
}