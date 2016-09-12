using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Ui.Converters
{
	public class HexColorToSolidBrushConverter : IValueConverter
	{
		public static Color HexToColor(string hexColor) => (Color)ColorConverter.ConvertFromString(hexColor);
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new SolidColorBrush(HexToColor((string)value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
