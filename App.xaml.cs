using Microsoft.Maui.Platform;
using Vocon.Services.HotKeyService;

namespace Vocon
{
    public partial class App : Application
    {
        private readonly AppShell _shell;

        public App(AppShell shell)
        {
            InitializeComponent();
            _shell = shell;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(_shell);
            window.Created += (sender, e) =>
            {
                IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle((MauiWinUIWindow)window.Handler!.PlatformView!);
                var hotKeyService = IPlatformApplication.Current!.Services.GetRequiredService<HotKeyService>();
                hotKeyService.Start(hwnd);
                MessageHotkeyService.Attach(hotKeyService, hwnd);
            };

            return window;
        }
    }
}