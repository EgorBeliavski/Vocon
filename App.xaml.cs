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
                try
                {
                    IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle((MauiWinUIWindow)window.Handler!.PlatformView!);

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