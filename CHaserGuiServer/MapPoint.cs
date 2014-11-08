using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oika.Apps.CHaserGuiServer
{
    public struct MapPoint
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public MapPoint(int x, int y)
            : this()
        {
            this.X = x;
            this.Y = y;
        }

        public static bool TryParse(string text, out MapPoint point)
        {
            if (text == null)
            {
                point = new MapPoint();
                return false;
            }

            var vals = text.Split(',');
            if (vals.Length != 2)
            {
                point = new MapPoint();
                return false;
            }

            int x;
            int y;
            if (!int.TryParse(vals[0], out x) || !int.TryParse(vals[1], out y))
            {
                point = new MapPoint();
                return false;
            }

            point = new MapPoint(x, y);
            return true;
        }


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(MapPoint)) return false;

            var other = (MapPoint)obj;

            return this.X == other.X && this.Y == other.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public static bool operator == (MapPoint p1, MapPoint p2) {
            if ((object)p1 == null) return (object)p2 == null;
            return p1.Equals(p2);
        }

        public static bool operator != (MapPoint p1, MapPoint p2)
        {
            return !(p1 == p2);
        }

        public static IEnumerable<MapPoint> EnumerateAround(MapPoint point, bool containsSelf)
        {
            yield return point.ToUp().ToLeft();
            yield return point.ToUp();
            yield return point.ToUp().ToRight();
            yield return point.ToLeft();
            if (containsSelf) yield return point;
            yield return point.ToRight();
            yield return point.ToDown().ToLeft();
            yield return point.ToDown();
            yield return point.ToDown().ToRight();
        }

        #region インスタンスメソッド

        public MapPoint ToLeft()
        {
            return new MapPoint(X - 1, Y);
        }

        public MapPoint ToUp()
        {
            return new MapPoint(X, Y - 1);
        }

        public MapPoint ToRight()
        {
            return new MapPoint(X + 1, Y);
        }

        public MapPoint ToDown()
        {
            return new MapPoint(X, Y + 1);
        }

        public MapPoint Shift(DirectionKind direction)
        {
            switch (direction)
            {
                case DirectionKind.Up: return ToUp();
                case DirectionKind.Down: return ToDown();
                case DirectionKind.Left: return ToLeft();
                case DirectionKind.Right: return ToRight();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

    }
}
