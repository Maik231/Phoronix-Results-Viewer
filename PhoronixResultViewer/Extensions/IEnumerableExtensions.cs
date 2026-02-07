using System;
using System.Collections.Generic;
using System.Linq;

namespace PhoronixResultViewer.Extensions;

public static class IEnumerableExtensions
{
    public static double Geomean<T>(this IEnumerable<T> list, Func<T, double> converter)
    {
        var sumOfLogs = list.Sum(i => Math.Log(converter(i)));
        
        return Math.Exp(sumOfLogs / list.Count());
    }
}