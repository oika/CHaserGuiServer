using Oika.Libs.MeLogg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Oika.Apps.CHaserGuiServer
{
    public class MapFileInfo
    {
        public string Name { get; private set; }
        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }
        public MapPoint CoolStPoint { get; private set; }
        public MapPoint HotStPoint { get; private set; }
        public int TurnCount { get; private set; }
        public IList<CellKind[]> CellsList { get; private set; }

        private MapFileInfo()
        {
        }

        public static MapFileInfo Load(string filePath, ILogger logger)
        {
            if (!File.Exists(filePath))
            {
                logger.Warn("mapファイルなし：" + filePath);
                return null;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);

                var dic = new Dictionary<string, List<string>>();
                foreach (var line in lines)
                {
                    var idx = line.IndexOf(":");
                    if (idx <= 0) continue;
                    var key = line.Remove(idx);

                    if (!dic.ContainsKey(key)) dic[key] = new List<string>();
                    dic[key].Add(line.Substring(idx + 1));
                }

                return parseLines(dic, logger);
            }
            catch (UnauthorizedAccessException)
            {
                logger.Fatal("mapファイルの参照権限がありません：" + filePath);
                return null;
            }
            catch (IOException ioe)
            {
                logger.Fatal("mapファイル読込みエラー：" + ioe);
                return null;
            }
        }


        private static MapFileInfo parseLines(Dictionary<string, List<string>> lineDic, ILogger logger)
        {
            List<string> tmpLines;

            //size
            if (!lineDic.TryGetValue("S", out tmpLines) || tmpLines.Count == 0)
            {
                logger.Warn("mapサイズ指定なし");
                return null;
            }
            var sizeVal = tmpLines.First();
            MapPoint size;
            if (!MapPoint.TryParse(sizeVal, out size))
            {
                logger.Warn("mapサイズ指定不正");
                return null;
            }
            var width = size.X;
            var height = size.Y;

            //name
            var name = "";
            if (lineDic.TryGetValue("N", out tmpLines) && tmpLines.Any())
            {
                name = tmpLines.First();
            }

            //points
            if (!lineDic.TryGetValue("C", out tmpLines) || tmpLines.Count == 0)
            {
                logger.Warn("map始点指定なし");
                return null;
            }
            var coolPtVal = tmpLines.First();

            if (!lineDic.TryGetValue("H", out tmpLines) || tmpLines.Count == 0)
            {
                logger.Warn("map始点指定なし");
                return null;
            }
            var hotPtVal = tmpLines.First();

            MapPoint coolPt;
            MapPoint hotPt;
            if (!MapPoint.TryParse(coolPtVal, out coolPt) || !MapPoint.TryParse(hotPtVal, out hotPt))
            {
                logger.Warn("map始点指定不正");
                return null;
            }

            //turn count
            int turns;
            if (!lineDic.TryGetValue("T", out tmpLines) || tmpLines.Count == 0 || !int.TryParse(tmpLines.First(), out turns))
            {
                logger.Warn("mapターン数指定不正");
                return null;
            }

            //cells
            if (!lineDic.TryGetValue("D", out tmpLines) || tmpLines.Count < height)
            {
                logger.Warn("mapセル指定不足");
                return null;
            }

            var cellsList = new List<CellKind[]>();
            foreach (var line in tmpLines)
            {
                var cells = line.Split(',')
                                .Select(p => p.FirstOrDefault())
                                .Select(c => parseCellKind(c))
                                .ToArray();
                if (cells.Any(c => c == CellKind.Unknown))
                {
                    logger.Warn("mapセル指定不正");
                    return null;
                }
                cellsList.Add(cells);
            }


            return new MapFileInfo
            {
                CellsList = cellsList,
                RowCount = height,
                ColumnCount = width,
                CoolStPoint = coolPt,
                HotStPoint = hotPt,
                Name = name,
                TurnCount = turns,
            };
        }


        private static CellKind parseCellKind(char c)
        {
            return c == CellKind.Nothing.ToSendChar(null) ? CellKind.Nothing
                : c == CellKind.Block.ToSendChar(null) ? CellKind.Block
                : c == CellKind.Item.ToSendChar(null) ? CellKind.Item
                : CellKind.Unknown;
        }
    }
}
