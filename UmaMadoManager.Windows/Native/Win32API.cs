using System;
using System.Runtime.InteropServices;

namespace UmaMadoManager.Windows.Native
{
    public static class Win32API
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowInfo(IntPtr hWnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, int bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, WinEventProc pfnWinEventProc, int idProcess, int idThread, int dwflags);

        public delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int UnhookWinEvent(IntPtr hWinEventHook);

        // For debugging
        [DllImport("kernel32.dll")]
        public static extern int AllocConsole();

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

        public enum WinEventFlag : uint
        {
            WINEVENT_INCONTEXT = 0x0004,
            WINEVENT_OUTOFCONTEXT = 0x0000,
            WINEVENT_SKIPOWNPROCESS = 0x0002,
            WINEVENT_SKIPOWNTHREAD = 0x0001,
        }

        public const int EVENT_SYSTEM_FOREGROUND = 0x00000003;
        public const int EVENT_SYSTEM_MOVESIZEEND = 0x0000000b;
        public const int EVENT_SYSTEM_MINIMIZESTART = 0x00000016;
        public const int EVENT_SYSTEM_MINIMIZEEND = 0x00000017;
        public const int EVENT_OBJECT_LOCATIONCHANGE = 0x0000800b;
    }
}
