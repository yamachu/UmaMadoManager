using System;

namespace UmaMadoManager.Core.Services
{
    // FIXME: Windows依存っぽい実装だからいい感じにする
    public interface IAudioManager
    {
        void SetMute(IntPtr hWnd, bool isMute);
    }
}
