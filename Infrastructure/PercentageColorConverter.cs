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
            double val = value switch
            {
                double d => d,
                decimal dc => (double)dc,
                _ => 0
            };

            if (val > 0) return new SolidColorBrush(Color.FromRgb(34, 139, 34));
            if (val < 0) return new SolidColorBrush(Color.FromRgb(220, 20, 60));
            return new SolidColorBrush(Color.FromRgb(128, 128, 128)); 
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
