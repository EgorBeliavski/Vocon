using Microsoft.Maui.Platform;
using Vocon.Services.HotKeyService;

namespace Vocon
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            
        }


        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(new AppShell());
            window.Created += (sender, e) => {
                IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle((MauiWinUIWindow)window.Handler!.PlatformView!);
                var hotKeyService = IPlatformApplication.Current!.Services.GetRequiredService<HotKeyService>();
                hotKeyService.Start(hwnd);
                MessageHotleyService.Attach(hotKeyService, hwnd);

            };
            
            return window;
        }
    }
}