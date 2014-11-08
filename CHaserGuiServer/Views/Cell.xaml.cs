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
    /// Cell.xaml の相互作用ロジック
    /// </summary>
    public partial class Cell : UserControl
    {
        public static readonly DependencyProperty ValueProperty
                = DependencyProperty.Register("Value", typeof(CellKind), typeof(Cell),
                                      new FrameworkPropertyMetadata(CellKind.Nothing));

        public CellKind Value
        {
            get
            {
                return (CellKind)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }


        public Cell()
        {
            InitializeComponent();
        }
    }
}
