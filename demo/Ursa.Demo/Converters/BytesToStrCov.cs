using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Toll.Core.Common;

namespace Ursa.Demo.Converters;

public class BytesToStrCov : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte[] t)
        {
            return BitConverter.ToString(t).Replace("-", " ");
        }

        return "erro";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}