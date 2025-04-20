using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CryptoTrackerApp.Infrastructure
{
    public class PercentageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalPercent)
            {
                double percent = (double)decimalPercent;
                if (parameter is string param && param == "Background")
                {
                    return percent > 0
                        ? new SolidColorBrush(Color.FromRgb(33, 192, 95))
                        : percent < 0
                            ? new SolidColorBrush(Color.FromRgb(246, 70, 93))
                            : new SolidColorBrush(Color.FromRgb(154, 161, 176));
                }
                return percent > 0
                    ? new SolidColorBrush(Color.FromRgb(33, 192, 95))
                    : percent < 0
                        ? new SolidColorBrush(Color.FromRgb(246, 70, 93))
                        : new SolidColorBrush(Color.FromRgb(154, 161, 176));
            }
            else if (value is double doublePercent)
            {
                if (parameter is string param && param == "Background")
                {
                    return doublePercent > 0
                        ? new SolidColorBrush(Color.FromRgb(33, 192, 95))
                        : doublePercent < 0
                            ? new SolidColorBrush(Color.FromRgb(246, 70, 93))
                            : new SolidColorBrush(Color.FromRgb(154, 161, 176));
                }

                return doublePercent > 0
                    ? new SolidColorBrush(Color.FromRgb(33, 192, 95))
                    : doublePercent < 0
                        ? new SolidColorBrush(Color.FromRgb(246, 70, 93)) 
                        : new SolidColorBrush(Color.FromRgb(154, 161, 176));
            }

            return new SolidColorBrush(Colors.Gray);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}