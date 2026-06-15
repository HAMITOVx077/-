using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using МаршрутСборки.Models;

namespace МаршрутСборки.Helpers
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as string;
            return status switch
            {
                AssemblyStatus.New => new SolidColorBrush(Color.FromRgb(99, 102, 241)),
                AssemblyStatus.WaitingComponents => new SolidColorBrush(Color.FromRgb(234, 179, 8)),
                AssemblyStatus.InProgress => new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                AssemblyStatus.ReadyForTest => new SolidColorBrush(Color.FromRgb(168, 85, 247)),
                AssemblyStatus.OnTesting => new SolidColorBrush(Color.FromRgb(139, 92, 246)),
                AssemblyStatus.Ready => new SolidColorBrush(Color.FromRgb(34, 197, 94)),
                AssemblyStatus.Shipped => new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                AssemblyStatus.Rework => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class PriorityColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var priority = value as string;
            return priority switch
            {
                AssemblyPriority.Low => new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                AssemblyPriority.Medium => new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                AssemblyPriority.High => new SolidColorBrush(Color.FromRgb(234, 179, 8)),
                AssemblyPriority.Critical => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class WarrantyStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as string;
            return status switch
            {
                WarrantyStatus.Received => new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                WarrantyStatus.Diagnosing => new SolidColorBrush(Color.FromRgb(234, 179, 8)),
                WarrantyStatus.InRepair => new SolidColorBrush(Color.FromRgb(168, 85, 247)),
                WarrantyStatus.ReadyForPickup => new SolidColorBrush(Color.FromRgb(34, 197, 94)),
                WarrantyStatus.Closed => new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = value is bool b && b;
            return boolValue
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = value is bool b && b;
            return boolValue
                ? System.Windows.Visibility.Collapsed
                : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class StockLevelColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLow && isLow)
                return new SolidColorBrush(Color.FromRgb(239, 68, 68));
            return new SolidColorBrush(Color.FromRgb(34, 197, 94));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string)
                ? System.Windows.Visibility.Collapsed
                : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class NotNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}