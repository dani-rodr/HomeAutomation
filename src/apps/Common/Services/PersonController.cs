namespace HomeAutomation.apps.Common.Services;

public interface IPersonController
{
    void SetHome();
    void SetAway();
}

public class PersonController(IPersonEntities entities, IServices services, ILogger logger)
    : AutomationBase(logger),
        IPersonController
{
    private readonly PersonEntity _person = entities.Person;
    private readonly CounterEntity _counter = entities.Counter;
    private readonly ButtonEntity _toggle = entities.ToggleLocation;

    public void SetHome()
    {
        if (_person.IsAway())
        {
            Logger.LogInformation(
                "{PersonName} arrived home. Updating location and incrementing counter.",
                _person.Attributes?.FriendlyName ?? "Unknown person"
            );
            SetLocation(HaEntityStates.HOME);
            _counter.Increment();
        }
    }

    public void SetAway()
    {
        if (_person.IsHome())
        {
            Logger.LogInformation(
                "{PersonName} left home. Updating location and decrementing counter.",
                _person.Attributes?.FriendlyName ?? "Unknown person"
            );
            SetLocation(HaEntityStates.AWAY);
            _counter.Decrement();
        }
    }

    protected override IEnumerable<IDisposable> GetAutomations() =>
        [_toggle.StateChanges().IsValidButtonPress().Subscribe(ToggleLocation)];

    private void ToggleLocation(StateChange e)
    {
        Logger.LogInformation("Toggle button pressed. Current state: {State}", _person.State);
        if (_person.IsHome())
        {
            SetAway();
        }
        else
        {
            SetHome();
        }
    }

    private void SetLocation(string location) =>
        services.DeviceTracker.See(devId: _person.Attributes?.Id, locationName: location);
}
