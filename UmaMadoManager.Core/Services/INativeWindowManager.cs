using System;
using System.Collections.Generic;
using UmaMadoManager.Core.Models;

namespace UmaMadoManager.Core.Services
{
    // FIXME: Windows依存っぽい実装だからいい感じにする
    public interface INativeWindowManager
    {
        IntPtr GetWindowHandle(string windowName);
        WindowRect GetWindowRect(IntPtr hWnd);
        void ResizeWindow(IntPtr hWnd, WindowRect rect);
        IEnumerable<Screen> GetScreens();
    }
}
