using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Oika.Apps.CHaserGuiServer.Converters
{
    [ValueConversion(typeof(CellKind), typeof(string))]
    public class CellKindToDispCharConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = (CellKind)value;
            return val == CellKind.Block ? "■"
                    : val == CellKind.Cool ? "C"
                    : val == CellKind.Hot ? "H"
                    : val == CellKind.CoolAndHot ? "CH"
                    : val == CellKind.Item ? "◇"
                    : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = (string)value;
            return val == "■" ? CellKind.Block
                : val == "C" ? CellKind.Cool
                : val == "H" ? CellKind.Hot
                : val == "CH" ? CellKind.CoolAndHot
                : val == "◇" ? CellKind.Item
                : val == "" ? CellKind.Nothing
                : CellKind.Unknown;

        }
    }
}
