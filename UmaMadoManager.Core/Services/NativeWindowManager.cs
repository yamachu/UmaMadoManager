using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using UmaMadoManager.Core.Models;

namespace UmaMadoManager.Core.Services
{
    public class NativeWindowManager : INativeWindowManager
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowInfo(IntPtr hWnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, int bRepaint);

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public int cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public int dwStyle;
            public int dwExStyle;
            public int dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public short atomWindowType;
            public short wCreatorVersion;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private Screen[] _screens;

        public NativeWindowManager(Screen[] screens)
        {
            _screens = screens;
        }

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
            var ret = NativeWindowManager.GetWindowInfo(hWnd, ref p);

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

        public IEnumerable<Screen> GetScreens()
        {
            return _screens;
        }
    }
}
