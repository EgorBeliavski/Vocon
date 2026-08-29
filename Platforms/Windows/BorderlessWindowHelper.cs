using System.Runtime.InteropServices;

namespace Vocon.Platforms.Windows;

public static class BorderlessWindowHelper
{
    const int WM_NCCALCSIZE = 0x0083;
    const int GWLP_WNDPROC = -4;

    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_NOMOVE = 0x0002;
    const uint SWP_NOZORDER = 0x0004;
    const uint SWP_NOACTIVATE = 0x0010;
    const uint SWP_FRAMECHANGED = 0x0020;

    delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    static WndProcDelegate? _newWndProc;
    static IntPtr _oldWndProc = IntPtr.Zero;
    static bool _isSubclassed = false;

    [DllImport("user32.dll")]
    static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr newProc);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    static extern int SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr newProc);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("dwmapi.dll")]
    static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

    [StructLayout(LayoutKind.Sequential)]
    struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    public static void RemoveHairlineBorder(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero || _isSubclassed)
            return;                         

        var margins = new MARGINS();
        DwmExtendFrameIntoClientArea(hwnd, ref margins);

        _newWndProc ??= WndProc;
        var newProcPtr = Marshal.GetFunctionPointerForDelegate(_newWndProc);

        _oldWndProc = IntPtr.Size == 8
            ? SetWindowLongPtr64(hwnd, GWLP_WNDPROC, newProcPtr)
            : (IntPtr)SetWindowLong32(hwnd, GWLP_WNDPROC, newProcPtr);

        _isSubclassed = true;

        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);
    }

    static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (_oldWndProc == IntPtr.Zero)
            return IntPtr.Zero;

        if (msg == WM_NCCALCSIZE && wParam != IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }
}