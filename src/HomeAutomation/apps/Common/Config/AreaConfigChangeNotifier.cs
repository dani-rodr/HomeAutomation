using System.Reactive.Subjects;

namespace HomeAutomation.apps.Common.Config;

public sealed class AreaConfigChangeNotifier : IAreaConfigChangeNotifier, IDisposable
{
    private readonly Subject<AreaConfigChangedEvent> _changes = new();

    public IObservable<AreaConfigChangedEvent> Changes => _changes;

    public void Publish(AreaConfigChangedEvent changeEvent)
    {
        _changes.OnNext(changeEvent);
    }

    public void Dispose()
    {
        _changes.Dispose();
    }
}
