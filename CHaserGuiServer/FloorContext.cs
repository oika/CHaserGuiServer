using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Oika.Apps.CHaserGuiServer
{
    public class FloorContext
    {
        readonly Dictionary<MapPoint, CellKind> cellDic = new Dictionary<MapPoint, CellKind>();

        MapPoint coolPoint;
        MapPoint hotPoint;

        public CellKind this[int x, int y]
        {
            get
            {
                return cellDic[new MapPoint(x, y)];
            }
        }

        #region コンストラクタ

        public FloorContext(IList<CellKind[]> cellsList, MapPoint coolPt, MapPoint hotPt)
        {
            var rowCount = cellsList.Count;
            if (rowCount == 0) throw new ArgumentException("1行以上の情報が必要です");

            var columnCount = cellsList[0].Length;
            if (cellsList.Any(l => l.Length != columnCount)) throw new ArgumentException("列数の不足している行があります");

            for (int ri = 0; ri < rowCount; ri++)
            {
                var row = cellsList[ri];

                for (int ci = 0; ci < columnCount; ci++)
                {
                    cellDic.Add(new MapPoint(ci, ri), row[ci]);
                }
            }

            Update(coolPt, CellKind.Cool);
            this.coolPoint = coolPt;

            Update(hotPt, CellKind.Hot);
            this.hotPoint = hotPt;

            if (cellDic.Count(c => c.Value == CellKind.Cool) != 1) throw new ArgumentException("Coolを同定できません");
            if (cellDic.Count(c => c.Value == CellKind.Hot) != 1) throw new ArgumentException("Hotを同定できません");
        }

        #endregion

        #region 終了判定

        public bool IsGameSet()
        {
            foreach (var isCool in new[] { true, false })
            {
                var pt = isCool ? coolPoint : hotPoint;

                CellKind cl;
                if (!cellDic.TryGetValue(pt, out cl)) return true;  //エリア外にいるとき

                if (cl == CellKind.Block) return true;  //ブロックにつぶされたとき

                var myKind = isCool ? CellKind.Cool : CellKind.Hot;
                Debug.Assert(cl == myKind || cl == CellKind.CoolAndHot);

                //上下左右が壁のとき
                var square = new[] { pt.ToUp(), pt.ToLeft(), pt.ToRight(), pt.ToDown() };
                if (square.All(p => !cellDic.TryGetValue(p, out cl) || cl == CellKind.Block))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region 実行系公開メソッド

        public bool Update(MapPoint point, CellKind value)
        {
            if (!cellDic.ContainsKey(point)) return false;  //マップ外

            cellDic[point] = value;
            return true;
        }

        public CellKind[] Search(bool isCool, DirectionKind direction) 
        {
            var pt = isCool ? coolPoint : hotPoint;
            if (cellDic[pt] == CellKind.Block) throw new InvalidOperationException();

            var rtn = new CellKind[9];

            var tmpPt = pt;
            for (int i = 0; i < rtn.Length; i++)
            {
                tmpPt = tmpPt.Shift(direction);

                CellKind cell;
                if (!cellDic.TryGetValue(tmpPt, out cell)) cell = CellKind.Block;
                rtn[i] = cell;
            }

            return rtn;
        }

        public bool Walk(bool isCool, DirectionKind direction)
        {
            var fromPt = isCool ? coolPoint : hotPoint;
            if (cellDic[fromPt] == CellKind.Block) throw new InvalidOperationException();

            var destPt = fromPt.Shift(direction);

            var me = isCool ? CellKind.Cool : CellKind.Hot;
            var opp = isCool ? CellKind.Hot : CellKind.Cool;

            CellKind fromOldKind;
            CellKind destOldKind;

            //移動先 変更前の値
            if (!cellDic.TryGetValue(destPt, out destOldKind)) destOldKind = CellKind.Block;
            Debug.Assert(destOldKind != me);

            //移動元 変更前の値
            fromOldKind = cellDic[fromPt];
            Debug.Assert(fromOldKind == me || fromOldKind == CellKind.CoolAndHot);

            CellKind fromNewKind;
            CellKind destNewKind;
            var gotItem = false;

            if (destOldKind == CellKind.Item)
            {
                fromNewKind = CellKind.Block;
                destNewKind = me;
                gotItem = true;
            }
            else if (destOldKind == opp)
            {
                fromNewKind = CellKind.Nothing;
                destNewKind = CellKind.CoolAndHot;
            }
            else if (destOldKind == CellKind.Block)
            {
                fromNewKind = fromOldKind == me ? CellKind.Nothing : opp;
                destNewKind = CellKind.Block;
            }
            else
            {
                fromNewKind = fromOldKind == me ? CellKind.Nothing : opp;
                destNewKind = me;
            }

            Update(fromPt, fromNewKind);
            Update(destPt, destNewKind);

            if (isCool)
            {
                coolPoint = destPt;
            }
            else
            {
                hotPoint = destPt;
            }

            return gotItem;
        }


        public CellKind[] GetAroundInfo(bool isCool)
        {
            var pt = isCool ? coolPoint : hotPoint;

            return getAroundOf(pt);
        }


        public CellKind[] Look(bool isCool, DirectionKind direction)
        {
            var pt = isCool ? coolPoint : hotPoint;
            if (cellDic[pt] == CellKind.Block) throw new InvalidOperationException();

            return getAroundOf(pt.Shift(direction).Shift(direction));
        }

        public void Put(bool isCool, DirectionKind direction)
        {
            var pt = isCool ? coolPoint : hotPoint;
            if (cellDic[pt] == CellKind.Block) throw new InvalidOperationException();

            Update(pt.Shift(direction), CellKind.Block);
        }

        #endregion

        #region プライベートメソッド

        private CellKind[] getAroundOf(MapPoint pt)
        {
            return MapPoint.EnumerateAround(pt, true).Select(p =>
            {
                CellKind cl;
                if (!cellDic.TryGetValue(p, out cl)) cl = CellKind.Block;
                return cl;

            }).ToArray();
        }

        #endregion
    }
}
