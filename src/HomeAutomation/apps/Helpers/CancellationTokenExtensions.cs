using System.Reactive.Disposables;

namespace HomeAutomation.apps.Helpers;

public static class CancellationTokenExtensions
{
    public static IObservable<long> AsObservable(this CancellationToken token)
    {
        return Observable.Create<long>(observer =>
        {
            if (token.IsCancellationRequested)
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }

            var registration = token.Register(() => observer.OnCompleted());
            return registration;
        });
    }
}
