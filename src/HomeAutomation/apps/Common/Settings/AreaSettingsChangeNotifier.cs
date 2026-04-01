using System.Reactive.Subjects;

namespace HomeAutomation.apps.Common.Settings;

public sealed class AreaSettingsChangeNotifier : IAreaSettingsChangeNotifier, IDisposable
{
    private readonly Subject<AreaSettingsChangedEvent> _changes = new();

    public IObservable<AreaSettingsChangedEvent> Changes => _changes;

    public void Publish(AreaSettingsChangedEvent changeEvent)
    {
        _changes.OnNext(changeEvent);
    }

    public void Dispose()
    {
        _changes.Dispose();
    }
}
