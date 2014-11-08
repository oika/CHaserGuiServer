using Oika.Apps.CHaserGuiServer.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Oika.Apps.CHaserGuiServer.ViewModels
{
    public class MapPanelViewModel : ViewModelBase
    {
        public Size MapSize { get; private set; }

        private FloorContext _cells;

        public FloorContext Cells
        {
            get
            {
                return _cells;
            }
            private set
            {
                if (object.ReferenceEquals(_cells, value)) return;
                _cells = value;
                RaisePropertyChanged<MapPanelViewModel, FloorContext>(me => me.Cells);
            }
        }

        readonly Dictionary<bool, int> isCool_itemsDic;

        public int GetItemCount(bool isCool)
        {
            return isCool_itemsDic[isCool];
        }


        public MapPanelViewModel(MapFileInfo info)
        {
            this.MapSize = new Size(info.ColumnCount, info.RowCount);

            this.Cells = new FloorContext(info.CellsList, info.CoolStPoint, info.HotStPoint);

            isCool_itemsDic = new Dictionary<bool, int> 
            {
                { true, 0 }, { false, 0 }
            };
        }




        #region FloorContextをラップするメソッド

        public CellKind[] GetAroundInfo(bool isCool)
        {
            return this.Cells.GetAroundInfo(isCool);
        }


        public GameResultKind GetResult()
        {
            return Cells.GetResult();
        }

        #endregion


        public CellKind[] InvokeCall(bool isCool, MethodKind method, DirectionKind direction)
        {
            //非更新メソッド
            if (method == MethodKind.Look)
            {
                return Cells.Look(isCool, direction);
            }
            if (method == MethodKind.Search)
            {
                return Cells.Search(isCool, direction);
            }

            //更新メソッド
            if (method == MethodKind.Put)
            {
                Cells.Put(isCool, direction);
            }
            else if (method == MethodKind.Walk)
            {
                var gotItem = Cells.Walk(isCool, direction);
                if (gotItem) isCool_itemsDic[isCool]++;
            }
            else
            {
                throw new ArgumentException(method + "は不明なメソッド種別です");
            }
            // //更新通知
            RaisePropertyChanged<MapPanelViewModel, FloorContext>(me => me.Cells);
            return Cells.GetAroundInfo(isCool);
        }
    }
}
