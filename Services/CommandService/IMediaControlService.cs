using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Devices.Enumeration;
using Windows.Media;
using Windows.Media.Control;



namespace Vocon.Services.CommandService
{
    

    

    [StructLayout(LayoutKind.Sequential)]
    struct InputStruct
    {
        public uint type;
        public KeyDetails keyboard;
        public long padding;
    }

   

    [StructLayout(LayoutKind.Sequential)]
    struct KeyDetails
    {
        public ushort Key_Code;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
        

    }
    public interface IMediaControlService
    {
        Task NextTrack();
        Task PreviousTrack();
        Task Repeat();
        Task SetPlayState(bool wantedstate);
    }
    public class MediaControlService : IMediaControlService
    {

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, InputStruct[] pInputs, int cbSize);


        private const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        private const int VK_MEDIA_NEXT_TRACK = 0xB0;
        private const int VK_MEDIA_PREV_TRACK = 0xB1;


        private async Task SendInputKey(ushort key_code){
            InputStruct[] unionStructs = new InputStruct[2];
            unionStructs[0].type =1;
            unionStructs[0].keyboard = new KeyDetails()
            {
                Key_Code = key_code,
                dwFlags = 0
            };
            unionStructs[0].padding = 0;

            unionStructs[1].type = 1;
            unionStructs[1].keyboard = new KeyDetails()
            {
                Key_Code = key_code,
                dwFlags = 0x0002
            };
            unionStructs[1].padding = 0;

            var countevents=SendInput(2, unionStructs, Marshal.SizeOf<InputStruct>());
            var error = Marshal.GetLastWin32Error();
            Debug.WriteLine($"SendInput sent {countevents} events");
            Debug.WriteLine($"SendInput sent {error} ");
        }

        private static async Task<bool> GetStatus()
        {
            var sessions = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            var curent = sessions.GetCurrentSession();
            if(curent is null){return false;}
            var sessionStatus = curent.GetPlaybackInfo();
            return sessionStatus.PlaybackStatus== GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing; ;
        }

        public async Task NextTrack(){await SendInputKey(VK_MEDIA_NEXT_TRACK);}
        public async Task PreviousTrack() { await SendInputKey(VK_MEDIA_PREV_TRACK); }
        public async Task Repeat() {
            
            var sessions = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            var curent = sessions.GetCurrentSession();
            if (curent is null)
            {
                return;
            }

            var isSupported = curent.GetPlaybackInfo().Controls.IsRepeatEnabled;
            if (!isSupported)
            {
                Debug.WriteLine("No accept");
                return;
            }
            bool result = await curent.TryChangeAutoRepeatModeAsync(MediaPlaybackAutoRepeatMode.Track);
            Debug.WriteLine($"TryChangeAutoRepeatModeAsync result: {result}");
            
            
            var newInfo = curent.GetPlaybackInfo();
            Debug.WriteLine($"AutoRepeatMode after change: {newInfo.AutoRepeatMode}");
        }


        public async Task SetPlayState(bool wantedstate){
            if(await GetStatus()!= wantedstate)
            {
                await SendInputKey(VK_MEDIA_PLAY_PAUSE);
            }
            
        }
    }
}
