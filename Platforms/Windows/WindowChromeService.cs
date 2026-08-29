using Microsoft.UI;
using Microsoft.UI.Windowing;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace Vocon.Platforms.Windows;

public class WindowChromeService
{
    IntPtr _hwnd;
    AppWindow? _appWindow;

    const int WM_NCLBUTTONDOWN = 0x00A1;
    const int HTCAPTION = 0x2;
    const int SW_MINIMIZE = 6;
    const int SW_MAXIMIZE = 3;
    const int SW_RESTORE = 9;

    [DllImport("user32.dll")]
    static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public void Attach(Microsoft.UI.Xaml.Window window)
    {
        _hwnd = WindowNative.GetWindowHandle(window);
        var id = Win32Interop.GetWindowIdFromWindow(_hwnd);
        _appWindow = AppWindow.GetFromWindowId(id);
    }

    public void StartDrag()
    {
        ReleaseCapture();
        SendMessage(_hwnd, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
    }

    public void Minimize() => ShowWindow(_hwnd, SW_MINIMIZE);

    public bool IsMaximized =>
        _appWindow?.Presenter is OverlappedPresenter p && p.State == OverlappedPresenterState.Maximized;

    public void ToggleMaximize() => ShowWindow(_hwnd, IsMaximized ? SW_RESTORE : SW_MAXIMIZE);

    public void Close() => _appWindow?.Destroy();
}