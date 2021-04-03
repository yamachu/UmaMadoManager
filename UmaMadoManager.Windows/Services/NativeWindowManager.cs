using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using UmaMadoManager.Core.Models;
using UmaMadoManager.Core.Services;
using static PInvoke.User32;

namespace UmaMadoManager.Windows.Services
{
    public class NativeWindowManager : INativeWindowManager
    {
        private SafeEventHookHandle hHook;
        private WinEventProc callback;

        public event EventHandler<bool> OnForeground;
        public event EventHandler<bool> OnMinimized;
        public event EventHandler OnMoveOrSizeChanged;
        public event EventHandler OnMessageSent;
        public event EventHandler OnBorderChanged;

        ~NativeWindowManager()
        {
            if (!hHook.IsInvalid && !hHook.IsClosed)
            {
                hHook.Close();
                callback = null;
            }
        }

        public void SetHook(string windowName)
        {
            if (!hHook.IsInvalid && !hHook.IsClosed)
            {
                hHook.Close();
                callback = null;
            }

            callback = WinEventProc(windowName);
            hHook = SetWinEventHook(WindowsEventHookType.EVENT_SYSTEM_FOREGROUND, WindowsEventHookType.EVENT_OBJECT_LOCATIONCHANGE, IntPtr.Zero, callback, 0, 0, WindowsEventHookFlags.WINEVENT_OUTOFCONTEXT | WindowsEventHookFlags.WINEVENT_SKIPOWNPROCESS);
            var err = Marshal.GetLastWin32Error();
            if (hHook.IsInvalid)
            {
                System.Console.WriteLine("Hook set failed...");
                System.Console.WriteLine($"error code: {err}");
            }
        }

        private WinEventProc WinEventProc(string windowName)
        {
            return (IntPtr hWinEventHook, WindowsEventHookType eventType, IntPtr hWnd, int idObject, int idChild, int dwEventThread, uint dwmsEventTime) =>
            {
                GetWindowThreadProcessId(hWnd, out var targetProcessId);
                var process = Process.GetProcessById((int)targetProcessId);
                var isTarget = process?.MainWindowTitle == windowName;
                switch (eventType)
                {
                    case WindowsEventHookType.EVENT_SYSTEM_FOREGROUND:
                        OnForeground?.Invoke(this, isTarget);
                        break;
                    case WindowsEventHookType.EVENT_SYSTEM_MINIMIZESTART:
                        if (!isTarget) return;
                        OnMinimized?.Invoke(this, true);
                        return;
                    case WindowsEventHookType.EVENT_SYSTEM_MINIMIZEEND:
                        if (!isTarget) return;
                        OnMinimized?.Invoke(this, false);
                        break;
                    case WindowsEventHookType.EVENT_SYSTEM_MOVESIZEEND:
                    case WindowsEventHookType.EVENT_OBJECT_LOCATIONCHANGE:
                    if (!isTarget) return;
                        OnMoveOrSizeChanged?.Invoke(this, null);
                        break;
                    case WindowsEventHookType.EVENT_OBJECT_REORDER:
                        if (!isTarget) return;
                        if (idObject != 0 /* WindowObjId*/) return;
                        OnBorderChanged?.Invoke(this, null);
                        break;
                    default:
                        if (isTarget)
                        {
                            OnMessageSent?.Invoke(this, null);
                        }
                        return;
                }
            };
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

        public (WindowRect window, WindowRect client) GetWindowRect(IntPtr hWnd)
        {
            var p = new WINDOWINFO();
            p.cbSize = (uint)Marshal.SizeOf<WINDOWINFO>();
            var ret = GetWindowInfo(hWnd, ref p);

            if (!ret)
            {
                return (WindowRect.Empty, WindowRect.Empty);
            }

            return (new WindowRect
            {
                Left = p.rcWindow.left,
                Top = p.rcWindow.top,
                Bottom = p.rcWindow.bottom,
                Right = p.rcWindow.right,
            }, new WindowRect
            {
                Left = p.rcClient.left,
                Top = p.rcClient.top,
                Bottom = p.rcClient.bottom,
                Right = p.rcClient.right,
            });
        }

        public void ResizeWindow(IntPtr hWnd, WindowRect rect)
        {
            MoveWindow(hWnd, rect.Left, rect.Top, rect.Width, rect.Height, true);
        }

        public void SetTopMost(IntPtr hWnd, bool doTop)
        {
            SetWindowPos(hWnd, (IntPtr)(doTop ? SpecialWindowHandles.HWND_TOPMOST : SpecialWindowHandles.HWND_NOTOPMOST), 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
        }

        public void RemoveBorder(IntPtr hWnd, bool doRemove)
        {
            var currentStyle = (SetWindowLongFlags)GetWindowLong(hWnd, WindowLongIndexFlags.GWL_STYLE);
            var borderStyle = (SetWindowLongFlags.WS_CAPTION | SetWindowLongFlags.WS_THICKFRAME | SetWindowLongFlags.WS_MINIMIZEBOX | SetWindowLongFlags.WS_MAXIMIZEBOX | SetWindowLongFlags.WS_SYSMENU);
            var nextStyle = doRemove ? currentStyle & ~borderStyle : currentStyle | borderStyle;
            SetWindowLong(hWnd, WindowLongIndexFlags.GWL_STYLE, nextStyle);
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOOWNERZORDER);
        }
    }
}
