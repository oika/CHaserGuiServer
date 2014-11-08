using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oika.Apps.CHaserGuiServer
{
    public enum GameResultKind
    {
        Unknown,
        Continue,
        CoolWon,
        HotWon,
        Draw,
    }

    public static class GameResultKindExt
    {
        public static string ToName(this GameResultKind result)
        {
            switch (result)
            {
                case GameResultKind.Unknown: return "不明";
                case GameResultKind.Continue: return "続行";
                case GameResultKind.CoolWon: return "Cool勝利";
                case GameResultKind.HotWon: return "Hot勝利";
                case GameResultKind.Draw: return "引き分け";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
