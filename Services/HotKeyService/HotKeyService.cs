using System.Runtime.InteropServices;

namespace Vocon.Services.HotKeyService
{
    public interface IHotKeyService
    {
        event Action<bool>? ChangeState;

       
        void ChangeHotKey(IReadOnlyCollection<uint> newKeys);

        void Start(IntPtr windowHandle, IReadOnlyCollection<uint>? initialKeys = null);
        void ResetToDefault();
        IReadOnlyCollection<uint> GetCurrentKeys();
    }

    public class HotKeyService : IDisposable, IHotKeyService
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYUP = 0x0105;

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        private const int VK_MENU = 0x12; private const int VK_LMENU = 0xA4; private const int VK_RMENU = 0xA5;
        private const int VK_CONTROL = 0x11; private const int VK_LCONTROL = 0xA2; private const int VK_RCONTROL = 0xA3;
        private const int VK_SHIFT = 0x10; private const int VK_LSHIFT = 0xA0; private const int VK_RSHIFT = 0xA1;
        private const int VK_LWIN = 0x5B; private const int VK_RWIN = 0x5C;

        private const int PROBE_HOTKEY_ID = 6768;

        public static readonly uint[] DefaultKeys = { 0x12, 0x20 };

      
        private static readonly List<HashSet<uint>> ReservedCombos = new()
        {
            new HashSet<uint> { 0x12, 0x09 },      
            new HashSet<uint> { 0x11, 0x12, 0x2E },  
            new HashSet<uint> { 0x11, 0x1B },       
            new HashSet<uint> { 0x5B, 0x4C },        
            new HashSet<uint> { 0x5B, 0x44 },        
            new HashSet<uint> { 0x5B, 0x09 },      
            new HashSet<uint> { 0x11, 0x10, 0x1B },  
        };

        private HashSet<uint> _targetKeys = new(DefaultKeys);
        private readonly HashSet<int> _pressedKeys = new();
        private bool _comboWasActive;

        private IntPtr _windowHandle;
        private IntPtr _hookHandle = IntPtr.Zero;
        private LowLevelKeyboardProc? _hookProc;

        public event Action<bool>? ChangeState;
        private bool _isActive;

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

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public void Start(IntPtr windowHandle, IReadOnlyCollection<uint>? initialKeys = null)
        {
            _windowHandle = windowHandle;

           
            if (initialKeys != null && initialKeys.Count > 0)
            {
                _targetKeys = new HashSet<uint>(initialKeys);
            }

            InstallHook();
        }

        public IReadOnlyCollection<uint> GetCurrentKeys() => _targetKeys.ToArray();

        public void ChangeHotKey(IReadOnlyCollection<uint> newKeys)
        {
            if (newKeys == null || newKeys.Count == 0)
            {
                throw new InvalidOperationException("Hotkey combination cannot be empty");
            }

            var newSet = new HashSet<uint>(newKeys);

            if (newSet.SetEquals(_targetKeys))
            {
                throw new InvalidOperationException("This is already your current hotkey");
            }

            foreach (var reserved in ReservedCombos)
            {
                if (newSet.SetEquals(reserved))
                {
                    throw new InvalidOperationException("This combination is reserved by Windows and can't be used");
                }
            }

            
            if (TryToModifierPlusKey(newKeys, out uint modBits, out uint vk))
            {
                bool probeOk = RegisterHotKey(_windowHandle, PROBE_HOTKEY_ID, modBits, vk);
                if (!probeOk)
                {
                    throw new InvalidOperationException("This combination is already registered by another application");
                }
                UnregisterHotKey(_windowHandle, PROBE_HOTKEY_ID);
            }
            

            _targetKeys = newSet;
            _pressedKeys.Clear();
            _comboWasActive = false;
        }

        public void ResetToDefault()
        {
            ChangeHotKey(DefaultKeys);
        }

        private static bool TryToModifierPlusKey(IReadOnlyCollection<uint> keys, out uint modifiers, out uint vk)
        {
            modifiers = 0;
            vk = 0;

            if (keys.Count != 2) return false;

            uint? nonModifier = null;
            foreach (var k in keys)
            {
                uint? mod = ToModBit((int)k);
                if (mod.HasValue)
                {
                    modifiers |= mod.Value;
                }
                else
                {
                    if (nonModifier.HasValue) return false; 
                    nonModifier = k;
                }
            }

            if (modifiers == 0 || !nonModifier.HasValue) return false;

            vk = nonModifier.Value;
            return true;
        }

        private static uint? ToModBit(int vkCode) => vkCode switch
        {
            VK_MENU or VK_LMENU or VK_RMENU => MOD_ALT,
            VK_CONTROL or VK_LCONTROL or VK_RCONTROL => MOD_CONTROL,
            VK_SHIFT or VK_LSHIFT or VK_RSHIFT => MOD_SHIFT,
            VK_LWIN or VK_RWIN => MOD_WIN,
            _ => null
        };

        private void InstallHook()
        {
            if (_hookHandle != IntPtr.Zero) return;

            _hookProc = HookCallback;
            _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, GetModuleHandle(null), 0);

            if (_hookHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to install global keyboard hook");
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool isDown = wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN;
                bool isUp = wParam == WM_KEYUP || wParam == WM_SYSKEYUP;

                if (isDown) _pressedKeys.Add(vkCode);
                else if (isUp) _pressedKeys.Remove(vkCode);

                bool shouldSwallow = false;

                if (isDown)
                {
                    bool comboActiveNow = _targetKeys.Count > 0 && _targetKeys.All(k => _pressedKeys.Contains((int)k));

                    if (comboActiveNow && !_comboWasActive)
                    {
                        _isActive = !_isActive;
                        ChangeState?.Invoke(_isActive);
                    }

                   
                    shouldSwallow = comboActiveNow && _targetKeys.Contains((uint)vkCode);

                    _comboWasActive = comboActiveNow;
                }
                else if (isUp)
                {
                    bool wasPartOfActiveCombo = _comboWasActive && _targetKeys.Contains((uint)vkCode);
                    _comboWasActive = _targetKeys.Count > 0 && _targetKeys.All(k => _pressedKeys.Contains((int)k));

                   
                    shouldSwallow = wasPartOfActiveCombo;
                }

                if (shouldSwallow)
                {
                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (_hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }
        }
    }
}