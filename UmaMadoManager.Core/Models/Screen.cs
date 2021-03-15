using System;
using System.Drawing;

namespace UmaMadoManager.Core.Models
{
    public struct Screen
    {
        public Rectangle Bounds;
        public Rectangle WorkingArea;

        public bool ContainsWindow(WindowRect rect)
        {
            // 含むというよりは左上の頂点がどこにあるかで判定するようにする
            return Bounds.Left <= rect.Left && rect.Left < Bounds.Right && Bounds.Top <= rect.Top && rect.Top < Bounds.Bottom;
        }

        public WindowRect MaxContainerbleWindowRect(WindowRect windowRect, WindowRect clientRect, WindowFittingStandard fittingStandard)
        {
            var marginHeight = (clientRect.Top - windowRect.Top) + (windowRect.Bottom - clientRect.Bottom);
            var marginWidth = (clientRect.Left - windowRect.Left) + (windowRect.Right - clientRect.Right);
            var workingAreaRatio = (double)(WorkingArea.Height - marginHeight) / (WorkingArea.Width - marginWidth);
            switch (windowRect.Direction)
            {
                case WindowDirection.Horizontal:
                    if (workingAreaRatio >= windowRect.AspectRatio)
                    {
                        var nextClientWidth = WorkingArea.Width - marginWidth;
                        var nextClientHeight = clientRect.AspectRatio * nextClientWidth;
                        return new WindowRect
                        {
                            Left = WorkingArea.Left,
                            Top = WorkingArea.Top,
                            Right = WorkingArea.Right,
                            Bottom = WorkingArea.Top + (int)nextClientHeight + marginHeight,
                        };
                    }
                    else
                    {
                        var nextClientHeight = WorkingArea.Bottom - WorkingArea.Top - marginHeight;
                        var nextClientWidth = nextClientHeight / clientRect.AspectRatio;
                        return new WindowRect
                        {
                            Left = WorkingArea.Left,
                            Top = WorkingArea.Top,
                            Right = WorkingArea.Left + (int)(nextClientWidth) + marginWidth,
                            Bottom = WorkingArea.Bottom,
                        };
                    }
                case WindowDirection.Vertical:
                    if (workingAreaRatio < windowRect.AspectRatio)
                    {
                        var nextClientHeight = WorkingArea.Height - marginHeight;
                        var nextClientWidth = nextClientHeight / clientRect.AspectRatio;
                        return new WindowRect
                        {
                            Left = fittingStandard == WindowFittingStandard.LeftTop ? WorkingArea.Left : WorkingArea.Right - (int)nextClientWidth - marginWidth,
                            Top = WorkingArea.Top,
                            Right = fittingStandard == WindowFittingStandard.LeftTop ? WorkingArea.Left + (int)nextClientWidth + marginWidth : WorkingArea.Right,
                            Bottom = WorkingArea.Bottom,
                        };
                    }
                    else
                    {
                        var nextClientWidth = WorkingArea.Width - marginWidth;
                        var nextClientHeight = clientRect.AspectRatio * nextClientWidth;
                        return new WindowRect
                        {
                            Left = WorkingArea.Left,
                            Top = WorkingArea.Top,
                            Right = WorkingArea.Right,
                            Bottom = WorkingArea.Top + (int)nextClientHeight + marginHeight,
                        };
                    }
                default:
                    throw new ArgumentException();
            }
        }
    }
}
