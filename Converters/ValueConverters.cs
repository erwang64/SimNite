using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SimNite.Converters;

public class BoolToStepBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCurrent && isCurrent)
            return new SolidColorBrush(Color.FromRgb(247, 198, 20)); // SimNitePrimaryBrush
        
        return new SolidColorBrush(Color.FromRgb(21, 24, 33)); // SimNiteSurfaceBrush
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToStepBorderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCompleted && isCompleted)
            return new SolidColorBrush(Color.FromRgb(247, 198, 20)); // SimNitePrimaryBrush
        
        return new SolidColorBrush(Color.FromRgb(42, 48, 66)); // SimNiteBorderBrush
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToStepTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCurrent && isCurrent)
            return new SolidColorBrush(Color.FromRgb(20, 20, 20)); // Dark text for current step
        
        return new SolidColorBrush(Color.FromRgb(230, 232, 238)); // SimNiteTextBrush
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCurrent && isCurrent)
            return FontWeights.SemiBold;
        
        return FontWeights.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToConnectorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCompleted && isCompleted)
            return new SolidColorBrush(Color.FromRgb(247, 198, 20)); // SimNitePrimaryBrush
        
        return new SolidColorBrush(Color.FromRgb(42, 48, 66)); // SimNiteBorderBrush
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class FileSizeConverter : IValueConverter
{
    private static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            if (bytes == 0) return "0 B";

            int mag = (int)Math.Log(bytes, 1024);
            // Cap to highest suffix block
            if (mag >= SizeSuffixes.Length) mag = SizeSuffixes.Length - 1;

            decimal adjustedSize = (decimal)bytes / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }
        
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
