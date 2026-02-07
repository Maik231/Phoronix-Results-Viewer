using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;
using PhoronixResultViewer.Models;

namespace PhoronixResultViewer.Converters;

public class ResultToDoubleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is List<Result> results)
        {
            return results.Select(r => r.Performance).ToList();
        }
        
        return new BindingNotification(new InvalidCastException(), 
            BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}