

namespace Vocon.Services.OverlayWindowService
{
    public interface IOverlayWindowService
    {
        void Show();
        void Hide();
        void UpdateState(bool isRecording, bool isProcessing);
    }
        public class OverlayWindowService : IOverlayWindowService
        {
            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

            [DllImport("user32.dll")]
            private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll")]
            private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
                int X, int Y, int cx, int cy, uint uFlags);

            [DllImport("user32.dll")]
            private static extern uint GetDpiForWindow(IntPtr hWnd);

            [DllImport("gdi32.dll")]
            private static extern IntPtr CreateEllipticRgn(int nLeftRect, int nTopRect,
                int nRightRect, int nBottomRect);

            [DllImport("user32.dll")]
            private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);
            [StructLayout(LayoutKind.Sequential)]
            private struct RECT
            {
                public int Left, Top, Right, Bottom;
            }

            [DllImport("user32.dll")]
            private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
            private const uint SWP_NOSIZE = 0x0001;
            private const uint SWP_NOACTIVATE = 0x0010;

            private const int GWL_EXSTYLE = -20;
            private const long WS_EX_NOACTIVATE = 0x08000000;
            private const long WS_EX_TOOLWINDOW = 0x00000080;
            private const int SW_SHOWNOACTIVATE = 4;

            private const int OverlayWidth = 40;
            private const int OverlayHeight = 40;

            private Microsoft.Maui.Controls.Window? _overlayWindow;
            private AppWindow? _appWindow;
            private IntPtr _hwnd;
            private OverlayViewModel? _viewModel;
            private bool _initialized;
            private bool _pendingShow;
            private bool _regionApplied;

            public void Show()
            {
                _pendingShow = true;

                if (!_initialized)
                {
                    EnsureCreated();
                    return;
                }

                if (_hwnd != IntPtr.Zero)
                {
                    PositionTopCenter();
                    ShowWindow(_hwnd, SW_SHOWNOACTIVATE);
                    ApplyRoundRegionDeferred();
                }
            }

            public void Hide()
            {
                _pendingShow = false;
                _appWindow?.Hide();
            }

            public void UpdateState(bool isRecording, bool isProcessing)
            {
                if (_viewModel == null) return;

                _viewModel.IsRecording = isRecording;
                _viewModel.IsProcessing = isProcessing;
            }

            private void EnsureCreated()
            {
                if (_initialized) return;
                _initialized = true;

                _viewModel = new OverlayViewModel();
                var page = new OverlayPage(_viewModel);
                _overlayWindow = new Microsoft.Maui.Controls.Window(page)
                {
                    Title = "VoconOverlay",
                    Width = OverlayWidth,
                    Height = OverlayHeight
                };

            _overlayWindow.HandlerChanged += (s, e) => OnNativeWindowReady();

                Microsoft.Maui.Controls.Application.Current?.OpenWindow(_overlayWindow);
            }

        private void OnNativeWindowReady()
        {
            if (_overlayWindow?.Handler?.PlatformView is not Microsoft.UI.Xaml.Window nativeWindow)
                return;

            _hwnd = WindowNative.GetWindowHandle(nativeWindow);
            var windowId = Win32Interop.GetWindowIdFromWindow(_hwnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            _appWindow.Hide();

            var contextMenuPresenter = OverlappedPresenter.CreateForContextMenu();
            _appWindow.SetPresenter(contextMenuPresenter);
            var dpi = GetDpiForWindow(_hwnd);
            var scale = dpi / 96.0;
            _appWindow.Resize(new Windows.Graphics.SizeInt32(
                (int)(OverlayWidth * scale), (int)(OverlayHeight * scale)));
            _appWindow.IsShownInSwitchers = false;

            var exStyle = GetWindowLongPtr(_hwnd, GWL_EXSTYLE).ToInt64();
            exStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
            SetWindowLongPtr(_hwnd, GWL_EXSTYLE, new IntPtr(exStyle));

            SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOACTIVATE);

            if (_pendingShow)
            {
                PositionTopCenter();
                ShowWindow(_hwnd, SW_SHOWNOACTIVATE);
                ApplyRoundRegionDeferred();
            }
        }


        private void ApplyRoundRegionDeferred()
        {
            if (_regionApplied || _hwnd == IntPtr.Zero) return;

            _ = Task.Delay(50).ContinueWith(_ =>
            {
                if (_overlayWindow?.Handler?.MauiContext == null) return;

                var dispatcher = _overlayWindow.Handler.MauiContext.Services
                    .GetService<IDispatcher>();

                dispatcher?.Dispatch(() =>
                {
                    if (!GetClientRect(_hwnd, out var rect)) return;

                    var width = rect.Right - rect.Left;
                    var height = rect.Bottom - rect.Top;

                    
                    var diameter = Math.Min(width, height);
                    var offsetX = (width - diameter) / 2;
                    var offsetY = (height - diameter) / 2;

                    var region = CreateEllipticRgn(offsetX, offsetY, offsetX + diameter, offsetY + diameter);
                    SetWindowRgn(_hwnd, region, true);
                    _regionApplied = true;
                });
            });
        }

        private void PositionTopCenter()
            {
                if (_appWindow == null) return;

                var displayArea = DisplayArea.GetFromWindowId(_appWindow.Id, DisplayAreaFallback.Primary);
                var workArea = displayArea.WorkArea;

                int x = workArea.X + (workArea.Width - _appWindow.Size.Width) / 2;
                int y = workArea.Y + 24;

                _appWindow.Move(new Windows.Graphics.PointInt32(x, y));
            }
        }
    }
