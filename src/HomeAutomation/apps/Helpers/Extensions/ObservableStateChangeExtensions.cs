using System.Linq;

namespace HomeAutomation.apps.Helpers.Extensions;

public static class ObservableStateChangeExtensions
{
    public static IObservable<(TLeft Left, TRight Right)> And<TLeft, TRight>(
        this IObservable<TLeft> left,
        IObservable<TRight> right
    ) => left.CombineLatest(right, (l, r) => (l, r));
}
