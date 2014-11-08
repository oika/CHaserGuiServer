using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Oika.Apps.CHaserGuiServer.Views
{
    /// <summary>
    /// MapPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class MapPanel : UserControl
    {
        public static readonly DependencyProperty MapSizeProperty
            = DependencyProperty.Register("MapSize", typeof(Size), typeof(MapPanel),
                                      new FrameworkPropertyMetadata(new Size(), onMapSizePropertyChanged));

        public Size MapSize
        {
            get
            {
                return (Size)GetValue(MapSizeProperty);
            }
            set
            {
                SetValue(MapSizeProperty, value);
            }
        }

        /// <summary>
        /// MapSizeProperty変更時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void onMapSizePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as MapPanel;

            obj.rebuildCells();
        }


        private void rebuildCells()
        {
            this.gridCells.Children.Clear();
            this.gridCells.RowDefinitions.Clear();
            this.gridCells.ColumnDefinitions.Clear();

            for (int ri = 0; ri < MapSize.Height; ri++)
            {
                gridCells.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (int ci = 0; ci < MapSize.Width; ci++)
            {
                gridCells.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            for (int ri = 0; ri < MapSize.Height; ri++)
            {
                for (int ci = 0; ci < MapSize.Width; ci++)
                {
                    var cell = new Cell();
                    cell.SetValue(Grid.RowProperty, ri);
                    cell.SetValue(Grid.ColumnProperty, ci);
                    gridCells.Children.Add(cell);
                    cell.SetBinding(Cell.ValueProperty, new Binding("Cells[" + ci + "," + ri + "]") { Mode = BindingMode.OneWay });
                }
            }


        }


        public MapPanel()
        {
            InitializeComponent();
        }


    }
}
