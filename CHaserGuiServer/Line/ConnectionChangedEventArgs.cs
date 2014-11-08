using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oika.Apps.CHaserGuiServer.Line
{
    public enum ConnectionState
    {
        Unknown,
        Connected,
        Disconnected,
        AbortedWithError,
    }

    public static class ConnectionStateExt
    {
        public static string ToDescription(this ConnectionState state)
        {
            switch (state)
            {
                case ConnectionState.Unknown: return "不明な接続状態";
                case ConnectionState.Connected: return "接続完了";
                case ConnectionState.Disconnected: return "切断";
                case ConnectionState.AbortedWithError: return "エラーによる接続破棄";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class ConnectionChangedEventArgs : EventArgs
    {
        public bool IsCool { get; private set; }
        public ConnectionState State { get; private set; }

        public ConnectionChangedEventArgs(bool isCool, ConnectionState state)
        {
            this.IsCool = isCool;
            this.State = state;
        }
    }
}
