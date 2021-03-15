using System;

namespace UmaMadoManager.Core.Models
{
    public struct WindowRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;
        public int Height => Bottom - Top;

        public double AspectRatio => (double)Height / Width * 1.0;

        public static WindowRect Empty => new WindowRect{
            Left = -1,
            Top = -1,
            Right = -1,
            Bottom = -1
        };

        public bool IsEmpty => Left == -1 && Top == -1 && Right == -1 && Bottom == -1;

        public WindowDirection Direction => (Right - Left) > (Bottom - Top) ? WindowDirection.Horizontal : WindowDirection.Vertical;

        public override string ToString()
        {
            return $"Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}";
        }
    }
}
