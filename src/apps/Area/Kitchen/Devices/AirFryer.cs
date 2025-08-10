namespace HomeAutomation.apps.Area.Kitchen.Devices;

public interface IAirFryer : IAutomation
{
    void Start();
    void Pause();
    void Stop();
    double Temperature { get; set; }
}

public class AirFryer(
    IAirFryerEntities entities,
    INotificationServices notificationServices,
    IEventHandler eventHandler,
    ILogger<AirFryer> logger
) : AutomationBase(logger), IAirFryer
{
    private readonly TimerEntity _timer = entities.Timer;
    private readonly INotificationServices _service = notificationServices;
    private readonly IEventHandler _handler = eventHandler;
    private readonly SensorEntity _status = entities.Status;
    private readonly IAirFryerEntities _entities = entities;

    protected override IEnumerable<IDisposable> GetAutomations() =>
        [
            _status.StateChanges().Subscribe(HandleStatusChange),
            _entities
                .CookingTime.StateChangesWithCurrent()
                .Subscribe(state =>
                {
                    var time = state?.New?.State ?? 0;
                    if (_status.State == AirFryerStatus.Running)
                    {
                        _timer.Restart(time);
                    }
                    else
                    {
                        _timer.SetDuration(time);
                    }
                }),
            _handler.OnMobileEvent(AirFryerNotificationAction.Start).Subscribe(),
            _handler.OnMobileEvent(AirFryerNotificationAction.Pause).Subscribe(),
            _handler.OnMobileEvent(AirFryerNotificationAction.Finish).Subscribe(),
        ];

    public void Start()
    {
        var time = _entities.CookingTime.State;
        if (!time.HasValue)
        {
            Logger.LogWarning("Cannot start Air Fryer without a cooking time");
            return;
        }
        _timer.SetDuration(time.Value);
        _entities.Start.Press();
        _timer.Start();
    }

    public void Pause()
    {
        _timer.Pause();
        _entities.Pause.Press();
    }

    public void Stop()
    {
        _timer.Finish();
        _entities.Stop.Press();
    }

    public double Temperature
    {
        get => _entities.Temperature.State ?? 0d;
        set => _entities.Temperature.SetNumericValue(value);
    }

    private void HandleStatusChange(StateChange e)
    {
        var newStatus = e.New?.State;
        var oldStatus = e.Old?.State;

        bool isRestarted =
            (oldStatus == AirFryerStatus.Standby || oldStatus == AirFryerStatus.Pause)
            && newStatus == AirFryerStatus.Running; // Simulating Actual Behavior

        int currentTimerDuration = _timer.GetDurationInSeconds();

        if (isRestarted)
        {
            var time = _entities.CookingTime.State ?? 0;

            _timer.Restart(time);
            _service.NotifyPocoF4(
                title: "Home Assistant",
                message: "Air Fryer is running",
                data: new
                {
                    chronometer = true,
                    when_relative = true,
                    timeout = currentTimerDuration,
                    when = currentTimerDuration,
                    tag = AirFryerNotificationAction.Tag,
                }
            );
            return;
        }

        switch (newStatus)
        {
            case AirFryerStatus.Pause:
            case AirFryerStatus.OnHold:
                _timer.Pause();
                _service.NotifyPocoF4(
                    title: "Home Assistant",
                    message: "Air Fryer is paused",
                    data: new { tag = AirFryerNotificationAction.Tag }
                );
                break;

            case AirFryerStatus.Running:
                _timer.Start();
                _service.NotifyPocoF4(
                    title: "Home Assistant",
                    message: "Air Fryer is running",
                    data: new
                    {
                        chronometer = true,
                        when_relative = true,
                        timeout = currentTimerDuration,
                        when = currentTimerDuration,
                        tag = AirFryerNotificationAction.Tag,
                    }
                );
                break;

            case AirFryerStatus.Completed:
            case AirFryerStatus.Standby:
            case AirFryerStatus.Unavailable:
                _timer.Finish();
                _service.NotifyPocoF4(
                    message: "Air Fryer is completed",
                    title: "Home Assistant",
                    data: new { tag = AirFryerNotificationAction.Tag }
                );
                break;
        }
    }
}

public static class AirFryerNotificationAction
{
    public const string Tag = "AIR_FRYER_TAG";
    public const string Start = "START";
    public const string Pause = "PAUSE";
    public const string Finish = "FINISH";
}

public static class AirFryerStatus
{
    public const string Standby = "Standby";
    public const string Pause = "Pause";
    public const string OnHold = "Pause By Removing Basket";
    public const string Running = "Program In Progress...";
    public const string Completed = "Completed";
    public const string Unavailable = "unavailable";
}
