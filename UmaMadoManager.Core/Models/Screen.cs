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
            return WorkingArea.Left <= rect.Left && rect.Left < WorkingArea.Right && WorkingArea.Top <= rect.Top && rect.Top < WorkingArea.Bottom;
        }

        public WindowRect MaxContainerbleWindowRect(WindowRect baseRect)
        {
            var workingAreaRatio = (double)WorkingArea.Height / WorkingArea.Width;
            switch (baseRect.Direction)
            {
                case WindowDirection.Horizontal:
                    if (workingAreaRatio > baseRect.AspectRatio)
                    {
                        var baseLength = (double)WorkingArea.Width / baseRect.Width;
                        return new WindowRect
                        {
                            Left = WorkingArea.Left,
                            Top = WorkingArea.Top,
                            Right = WorkingArea.Right,
                            Bottom = (int)Math.Floor(baseRect.Height * baseLength) + WorkingArea.Top,
                        };
                    }
                    else
                    {
                        var baseLength = (double)WorkingArea.Height / baseRect.Height;
                        return new WindowRect
                        {
                            Left = WorkingArea.Left,
                            Top = WorkingArea.Top,
                            Right = (int)Math.Floor(baseRect.Width * baseLength) + WorkingArea.Left,
                            Bottom = WorkingArea.Bottom,
                        };
                    }
                case WindowDirection.Vertical:
                    if (workingAreaRatio < baseRect.AspectRatio)
                    {
                        var baseLength = (double)WorkingArea.Height / baseRect.Height;
                        return new WindowRect
                        {
                            Left = WorkingArea.Left,
                            Top = WorkingArea.Top,
                            Right = (int)Math.Floor(baseRect.Width * baseLength) + WorkingArea.Left,
                            Bottom = WorkingArea.Bottom,
                        };
                    }
                    else
                    {
                        var baseLength = (double)WorkingArea.Width / baseRect.Width;
                        return new WindowRect
                        {
                            Left = WorkingArea.Left,
                            Top = WorkingArea.Top,
                            Right = WorkingArea.Right,
                            Bottom = (int)Math.Floor(baseRect.Height * baseLength) + WorkingArea.Top,
                        };
                    }
                default:
                    throw new ArgumentException();
            }
        }
    }
}
