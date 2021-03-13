using System;
using UmaMadoManager.Core.Models;

namespace UmaMadoManager.Core.Services
{
    // FIXME: Windows依存っぽい実装だからいい感じにする
    public interface INativeWindowManager
    {
        IntPtr GetWindowHandle(string windowName);
        WindowRect GetWindowRect(IntPtr hWnd);
        void ResizeWindow(IntPtr hWnd, WindowRect rect);
        void SetHook(string windowName);

        event EventHandler<bool> OnForeground;
        event EventHandler<bool> OnMinimized;
        event EventHandler OnMoveOrSizeChanged;
        event EventHandler OnMessageSent;
    }
}
