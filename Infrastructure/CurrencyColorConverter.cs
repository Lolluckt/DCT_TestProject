using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CryptoTrackerApp.Infrastructure
{
    public class CurrencyColorConverter : IValueConverter
    {
        private readonly string[] ColorPalette = {
            "#3498db", // Blue
            "#2ecc71", // Green
            "#e74c3c", // Red
            "#9b59b6", // Purple
            "#f1c40f", // Yellow
            "#1abc9c", // Turquoise
            "#34495e", // Dark Blue
            "#d35400", // Orange
            "#16a085", // Sea Green
            "#c0392b"  // Dark Red
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && !string.IsNullOrEmpty(text))
            {
                int hash = 0;
                foreach (char c in text)
                {
                    hash = (hash * 31) + c;
                }

                int index = Math.Abs(hash % ColorPalette.Length);
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(ColorPalette[index]));
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}