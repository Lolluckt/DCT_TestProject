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
            double dVal = 0;
            if (value is double d)
                dVal = d;
            else if (value is decimal dec)
                dVal = (double)dec;
            if (dVal > 5)
                return new SolidColorBrush(Color.FromRgb(0, 150, 0));
            else if (dVal > 0)
                return new SolidColorBrush(Color.FromRgb(60, 179, 113));
            else if (dVal < -5)
                return new SolidColorBrush(Color.FromRgb(178, 34, 34));
            else if (dVal < 0)
                return new SolidColorBrush(Color.FromRgb(220, 20, 60));
            else
                return new SolidColorBrush(Color.FromRgb(128, 128, 128));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}