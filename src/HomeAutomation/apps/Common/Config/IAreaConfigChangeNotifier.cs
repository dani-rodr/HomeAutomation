namespace HomeAutomation.apps.Common.Config;

public interface IAreaConfigChangeNotifier
{
    IObservable<AreaConfigChangedEvent> Changes { get; }

    void Publish(AreaConfigChangedEvent changeEvent);
}
