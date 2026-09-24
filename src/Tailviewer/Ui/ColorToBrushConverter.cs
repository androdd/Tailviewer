using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Tailviewer.Ui
{
	/// <summary>
	///     Converts a <see cref="Color" /> into a (frozen) <see cref="SolidColorBrush" /> so colors
	///     can be bound to properties such as <see cref="System.Windows.Shapes.Shape.Fill" />.
	/// </summary>
	public sealed class ColorToBrushConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Color color))
				return null;

			var brush = new SolidColorBrush(color);
			brush.Freeze();
			return brush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is SolidColorBrush brush)
				return brush.Color;

			return null;
		}
	}
}
