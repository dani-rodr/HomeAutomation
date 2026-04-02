namespace HomeAutomation.apps.Common.Settings;

public interface ILiveAppConfig<T> : IAppConfig<T>
    where T : class, new()
{
    T Settings { get; }

    IObservable<T> Changes { get; }
}
