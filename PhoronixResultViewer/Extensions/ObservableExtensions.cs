using System;
using System.Reactive;
using System.Reactive.Linq;

namespace PhoronixResultViewer.Extensions;

public static class ObservableExtensions
{
    public static IObservable<Unit> ToUnit<T>(this IObservable<T> obs)
    {
        return obs.Select(x => Unit.Default);
    }
}