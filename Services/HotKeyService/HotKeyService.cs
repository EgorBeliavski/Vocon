using System.Runtime.InteropServices;

namespace Vocon.Services.HotKeyService
{
    public class HotKeyService : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;

        private const int MAIN_HOTKEY_ID = 6767;
        private const int SIDE_HOTKEY_ID = 0x9090;

        private const uint MOD_ALT = 0x0001;
        private const uint VK_SPACE = 0x20;

        private IntPtr _window_handler;

        private bool _isActive = false;

        public Action<bool> ?ChangeState;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr _window_handler, int MAIN_HOTKEY_ID, uint MOD_ALT, uint VK_SPACE);


        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr _window_handler, int MAIN_HOTKEY_ID);


        public bool Start(IntPtr _windowHandle){
            _window_handler = _windowHandle;
            bool register=RegisterHotKey(_windowHandle, MAIN_HOTKEY_ID, MOD_ALT, VK_SPACE);

            if(register==false){
                throw new InvalidOperationException();
            }

            return register;
        }

        public bool HandleMessage(int INCOMING_MAIN_HOTKEY_ID,int INCOMING_WM_HOTKEY)
        {

            if (MAIN_HOTKEY_ID == INCOMING_MAIN_HOTKEY_ID && WM_HOTKEY == INCOMING_WM_HOTKEY)
            {
                _isActive = !_isActive;
                ChangeState?.Invoke(_isActive);
            }
            return _isActive;
        }

        public void Dispose(){
            UnregisterHotKey(_window_handler, MAIN_HOTKEY_ID);
        }
    }
}
