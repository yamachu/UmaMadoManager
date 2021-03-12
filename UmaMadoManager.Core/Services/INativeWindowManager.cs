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
    }
}
