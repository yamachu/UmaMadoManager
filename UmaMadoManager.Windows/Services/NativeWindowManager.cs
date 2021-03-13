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
        // Hookを設定した後のHandle
        private IntPtr hHook;
        private Native.Win32API.WinEventProc callback;

        public event EventHandler<bool> OnForeground;
        public event EventHandler<bool> OnMinimized;
        public event EventHandler OnMoveOrSizeChanged;
        public event EventHandler OnMessageSent;

        public NativeWindowManager()
        {
            callback = WinEventProc;
            hHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_OBJECT_LOCATIONCHANGE, IntPtr.Zero, callback, 0, 0, (int)(WinEventFlag.WINEVENT_OUTOFCONTEXT | WinEventFlag.WINEVENT_SKIPOWNPROCESS));
            var err = Marshal.GetLastWin32Error();
            if (hHook == IntPtr.Zero)
            {
                System.Console.WriteLine("Hook set failed...");
                System.Console.WriteLine($"error code: {err}");
            }
        }

        ~NativeWindowManager()
        {
            if (hHook != IntPtr.Zero)
            {
                UnhookWinEvent(hHook);
                hHook = IntPtr.Zero;
                callback = null;
            }
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            GetWindowThreadProcessId(hWnd, out var targetProcessId);
            var process = Process.GetProcessById((int)targetProcessId);
            var isTarget = process?.MainWindowTitle == "umamusume";
            switch (eventType)
            {
                case EVENT_SYSTEM_FOREGROUND:
                    OnForeground?.Invoke(this, isTarget);
                    break;
                case EVENT_SYSTEM_MINIMIZESTART:
                    OnMinimized?.Invoke(this, true);
                    return;
                case EVENT_SYSTEM_MINIMIZEEND:
                    OnMinimized?.Invoke(this, false);
                    break;
                case EVENT_SYSTEM_MOVESIZEEND:
                case EVENT_OBJECT_LOCATIONCHANGE:
                    OnMoveOrSizeChanged?.Invoke(this, null);
                    break;
                default:
                    if (isTarget)
                    {
                        OnMessageSent?.Invoke(this, null);
                    }
                    return;
            }
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
