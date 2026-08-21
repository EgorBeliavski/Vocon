using System.Runtime.InteropServices;


namespace Vocon.Services.HotKeyService
{
    internal class MessageHotkeyService
    {
        public delegate IntPtr CallBackMessage(IntPtr handler,uint MessageCode, IntPtr wParam, IntPtr lParam, IntPtr ID, IntPtr referenceData);



        [DllImport("comctl32.dll", SetLastError = true)]
        public static extern bool SetWindowSubclass(IntPtr handler, CallBackMessage message,IntPtr subclassId, IntPtr refdata);

        [DllImport("comctl32.dll")]
        public static extern IntPtr DefSubclassProc(IntPtr handler, uint MessageCode, IntPtr wParam, IntPtr lParam);

        private static CallBackMessage ?callback;



        public static void Attach(IHotKeyService service,IntPtr handle){
            callback = (IntPtr handler, uint MessageCode, IntPtr wParam, IntPtr lParam, IntPtr ID, IntPtr referenceData) => {
                return DefSubclassProc(handler,MessageCode,wParam,lParam);

            };
            bool subclassResult = SetWindowSubclass(handle,callback,IntPtr.Zero, IntPtr.Zero);
        }


    }
}
