using System;
using NAudio.CoreAudioApi; //Wasapi
using UmaMadoManager.Core.Services;
using static UmaMadoManager.Windows.Native.Win32API;

namespace UmaMadoManager.Windows.Services
{
    public class AudioManager : IAudioManager
    {
        public void SetMute(IntPtr hWnd, bool isMute)
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            GetWindowThreadProcessId(hWnd, out var processId);

            var sessions = device.AudioSessionManager.Sessions;
            var sessionCount = sessions.Count;

            for (var i = 0; i < sessionCount; i++)
            {
                var session = device.AudioSessionManager.Sessions[i];
                if (session.GetProcessID != processId)
                {
                    continue;
                }

                session.SimpleAudioVolume.Mute = isMute;
            }
        }
    }
}
