using System;
using UmaMadoManager.Core.Models;

namespace UmaMadoManager.Core.Services
{
    // FIXME: Windows依存っぽい実装だからいい感じにする
    public interface INativeWindowManager
    {
        IntPtr GetWindowHandle(string windowName);
        (WindowRect window, WindowRect client) GetWindowRect(IntPtr hWnd);
        void ResizeWindow(IntPtr hWnd, WindowRect rect);
        void SetHook(string windowName);
        void SetTopMost(IntPtr hWnd, bool doTop);
        void RemoveBorder(IntPtr hWnd, bool doRemove);

        event EventHandler<bool> OnForeground;
        event EventHandler<bool> OnMinimized;
        event EventHandler OnMoveOrSizeChanged;
        event EventHandler OnMessageSent;
        event EventHandler OnBorderChanged;
    }
}
