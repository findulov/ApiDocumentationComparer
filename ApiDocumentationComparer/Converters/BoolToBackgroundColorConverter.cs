using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ApiDocumentationComparer.Converters
{
    public class BoolToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = Color.FromRgb(255, 255, 255);

            if ((bool)value)
            {
                color = (Color)ColorConverter.ConvertFromString((string)parameter);
            }

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
