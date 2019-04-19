using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oika.Apps.CHaserGuiServer
{
    public enum CellKind
    {
        Unknown,
        Nothing,
        Block,
        Item,
        Cool,
        Hot,
        CoolAndHot,
    }

    public static class CellKindExt
    {
        public static char ToSendChar(this CellKind cell, bool? isCool)
        {
            switch (cell)
            {
                case CellKind.Nothing: return '0';
                case CellKind.Block: return '2';
                case CellKind.Item: return '3';
                case CellKind.Cool:
                    if (isCool == null) throw new ArgumentNullException(nameof(isCool));
                    return isCool.Value ? '0' : '1';  //※自身のセルは0として扱う
                case CellKind.Hot:
                    if (isCool == null) throw new ArgumentNullException(nameof(isCool));
                    return isCool.Value ? '1' : '0';
                case CellKind.CoolAndHot:
                     return '1';
                default:
                     throw new ArgumentOutOfRangeException();
            }
        }
    }
}
