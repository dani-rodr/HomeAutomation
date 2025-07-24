namespace HomeAutomation.apps.Common.Services;

public interface IPersonController
{
    public void SetHome();
    public void SetAway();
}

public class PersonController(IPersonEntities entities, IServices services)
    : AutomationBase(),
        IPersonController
{
    private readonly PersonEntity _person = entities.Person;
    private readonly CounterEntity _counter = entities.Counter;
    private readonly ButtonEntity _toggle = entities.ToggleLocation;

    public void SetHome()
    {
        if (_person.IsAway())
        {
            SetLocation(HaEntityStates.HOME);
            _counter.Increment();
        }
    }

    public void SetAway()
    {
        if (_person.IsHome())
        {
            SetLocation(HaEntityStates.AWAY);
            _counter.Decrement();
        }
    }

    protected override IEnumerable<IDisposable> GetAutomations() =>
        [_toggle.StateChanges().IsValidButtonPress().Subscribe(ToggleLocation)];

    private void ToggleLocation(StateChange e)
    {
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
