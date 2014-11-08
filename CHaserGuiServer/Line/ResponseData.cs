using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Oika.Apps.CHaserGuiServer.Line
{
    public class ResponseData
    {
        public bool IsGameSet { get; private set; }

        public ReadOnlyCollection<CellKind> Cells { get; private set; }

        public const int TotalLength = 10;

        public ResponseData(bool isGameSet, IList<CellKind> cells)
        {
            if (cells.Count + 1 != TotalLength) throw new ArgumentException();

            this.IsGameSet = isGameSet;
            this.Cells = new ReadOnlyCollection<CellKind>(cells);
        }

        public string ToSendChars()
        {
            var list = new List<char>();
            list.Add(IsGameSet ? '0' : '1');

            list.AddRange(Cells.Select(c => c.ToSendChar()));

            return new string(list.ToArray());
        }

    }
}
