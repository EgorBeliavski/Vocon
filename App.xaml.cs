using Microsoft.Maui.Platform;
using Vocon.Platforms.Windows;
using Vocon.Services.HotKeyService;
using WinRT.Interop;

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
                try
                {
                    var mauiWindow = (MauiWinUIWindow)window.Handler!.PlatformView!;
                    IntPtr hwnd = WindowNative.GetWindowHandle(mauiWindow);

                    BorderlessWindowHelper.RemoveHairlineBorder(hwnd);

                    var services = IPlatformApplication.Current!.Services;
                    var hotKeyService = services.GetRequiredService<IHotKeyService>();
                    var hotKeySettingsService = services.GetRequiredService<IHotKeySettingsService>();

                    var savedKeys = hotKeySettingsService.Load();
                    hotKeyService.Start(hwnd, savedKeys);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[App] EXCEPTION in window.Created: {ex}");
                }
            };
            return window;
        }
    }
}