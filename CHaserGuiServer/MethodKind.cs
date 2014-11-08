using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oika.Apps.CHaserGuiServer
{
    public enum MethodKind
    {
        Unknown,
        Walk,
        Look,
        Search,
        Put,
    }

    public static class MethodKindExt
    {
        public static string ToChar(this MethodKind method)
        {
            switch (method)
            {
                case MethodKind.Unknown: return "";
                case MethodKind.Walk: return "w";
                case MethodKind.Look: return "l";
                case MethodKind.Search: return "s";
                case MethodKind.Put: return "p";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
