using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using UmaMadoManager.Core.Models;
using UmaMadoManager.Core.Services;
using static UmaMadoManager.Windows.Native.Win32API;

namespace UmaMadoManager.Windows.Services
{
    public class NativeWindowManager : INativeWindowManager
    {
        public IntPtr GetWindowHandle(string windowName)
        {
            try
            {
                var process = Process.GetProcesses().First(x => x.MainWindowTitle == windowName);
                return process.MainWindowHandle;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                return IntPtr.Zero;
            }
        }

        public WindowRect GetWindowRect(IntPtr hWnd)
        {
            var p = new WINDOWINFO();
            p.cbSize = Marshal.SizeOf<WINDOWINFO>();
            var ret = GetWindowInfo(hWnd, ref p);

            if (ret == 0)
            {
                return WindowRect.Empty;
            }

            return new WindowRect
            {
                Left = p.rcWindow.left,
                Top = p.rcWindow.top,
                Bottom = p.rcWindow.bottom,
                Right = p.rcWindow.right,
            };
        }

        public void ResizeWindow(IntPtr hWnd, WindowRect rect)
        {
            SetForegroundWindow(hWnd);
            MoveWindow(hWnd, rect.Left, rect.Top, rect.Width, rect.Height, 1);
        }
    }
}
