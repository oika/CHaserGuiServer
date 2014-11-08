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
        public static char ToSendChar(this CellKind cell)
        {
            switch (cell)
            {
                case CellKind.Nothing: return '0';
                case CellKind.Block: return '2';
                case CellKind.Item: return '3';
                case CellKind.Cool:
                case CellKind.Hot:
                case CellKind.CoolAndHot:
                     return '1';
                default:
                     throw new ArgumentOutOfRangeException();
            }
        }
    }
}
