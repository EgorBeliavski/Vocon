using System.Runtime.InteropServices;

namespace Vocon.Services.HotKeyService
{
    public interface IHotKeyRecorderService
    {
        void StartRecording(Action<uint[]> onCaptured, Action? onCancelled = null);
        void StopRecording();
    }

    public class HotKeyRecorderService : IHotKeyRecorderService
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYUP = 0x0105;
        private const int VK_ESCAPE = 0x1B;

        private IntPtr _hookHandle = IntPtr.Zero;
        private LowLevelKeyboardProc? _hookProc;

        private Action<uint[]>? _onCaptured;
        private Action? _onCancelled;

        private readonly HashSet<int> _currentlyDown = new();
        private readonly HashSet<int> _comboBeingRecorded = new();

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        public void StartRecording(Action<uint[]> onCaptured, Action? onCancelled = null)
        {
            if (_hookHandle != IntPtr.Zero)
            {
                StopRecording();
            }

            _onCaptured = onCaptured;
            _onCancelled = onCancelled;
            _currentlyDown.Clear();
            _comboBeingRecorded.Clear();

            _hookProc = HookCallback;
            _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, GetModuleHandle(null), 0);

            if (_hookHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to install keyboard hook");
            }
        }

        public void StopRecording()
        {
            if (_hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }

            _hookProc = null;
            _onCaptured = null;
            _onCancelled = null;
            _currentlyDown.Clear();
            _comboBeingRecorded.Clear();
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool isDown = wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN;
                bool isUp = wParam == WM_KEYUP || wParam == WM_SYSKEYUP;

                if (isDown)
                {
                    if (vkCode == VK_ESCAPE && _currentlyDown.Count == 0)
                    {
                        var cancelled = _onCancelled;
                        StopRecording();
                        cancelled?.Invoke();
                        return (IntPtr)1;
                    }

                    _currentlyDown.Add(vkCode);
                    _comboBeingRecorded.Add(vkCode);
                }
                else if (isUp)
                {
                    _currentlyDown.Remove(vkCode);

                    if (_currentlyDown.Count == 0 && _comboBeingRecorded.Count > 0)
                    {
                        var result = _comboBeingRecorded.Select(k => (uint)k).ToArray();
                        var captured = _onCaptured;
                        StopRecording();
                        captured?.Invoke(result);
                    }
                }

                return (IntPtr)1;
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }
    }
}