using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Toll.Core.Common;
using Ursa.Controls;

namespace Ursa.Demo.Converters;

public class TrDirectionToStrCov : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DataDirection t)
        {
            return t switch
            {
                DataDirection.Receive => "收:",
                DataDirection.Send => "发:",
                _ => "-"
            };
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}