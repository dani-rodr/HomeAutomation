namespace HomeAutomation.apps.Common.Settings;

public interface IAreaSettingsChangeNotifier
{
    IObservable<AreaSettingsChangedEvent> Changes { get; }

    void Publish(AreaSettingsChangedEvent changeEvent);
}
