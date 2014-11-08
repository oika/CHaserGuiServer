using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oika.Apps.CHaserGuiServer
{
    public enum DirectionKind
    {
        Unknown,
        Up,
        Down,
        Left,
        Right,
    }

    public static class DirectionKindExt
    {
        public static string ToChar(this DirectionKind direction)
        {
            switch (direction)
            {
                case DirectionKind.Unknown: return "";
                case DirectionKind.Up: return "u";
                case DirectionKind.Down: return "d";
                case DirectionKind.Left: return "l";
                case DirectionKind.Right: return "r";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
