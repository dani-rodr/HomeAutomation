namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class MotionAutomation : MotionAutomationBase
{
    private readonly Entities _entities;
    private readonly DimmingLightController _dimmingController;

    protected override int SensorWaitTime => 30;
    protected override int SensorActiveDelayValue => 45;
    protected override int SensorInactiveDelayValue => 1;

    public MotionAutomation(
        Entities entities,
        SwitchEntity masterSwitch,
        BinarySensorEntity motionSensor,
        ILogger logger
    )
        : base(
            masterSwitch,
            motionSensor,
            entities.Light.SalaLightsGroup,
            logger,
            entities.Number.Ld2410Esp321StillTargetDelay
        )
    {
        _entities = entities;
        _dimmingController = new DimmingLightController(
            SensorActiveDelayValue,
            entities.Number.Ld2410Esp321StillTargetDelay,
            dimDelaySeconds: 15
        );
    }

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor.StateChanges().IsOn().Subscribe(e => _dimmingController.OnMotionDetected(Light));
        yield return MotionSensor
            .StateChanges()
            .IsOff()
            .Subscribe(async _ => await _dimmingController.OnMotionStoppedAsync(Light));
    }

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations()
    {
        yield return TurnOnMotionSensorOnTvOff();
        yield return TurnOnMotionSensorAfterNoMotionAndRoomOccupied();
    }

    protected override IEnumerable<IDisposable> GetAdditionalSwitchableAutomations()
    {
        yield return TurnOffPantryLights();
        yield return SetSensorDelayOnKitchenOccupancy();
    }

    private IDisposable TurnOnMotionSensorAfterNoMotionAndRoomOccupied()
    {
        return MotionSensor
            .StateChanges()
            .IsOffForMinutes(2)
            .Where(_ =>
                _entities.BinarySensor.ContactSensorDoor.IsClosed()
                && _entities.BinarySensor.BedroomPresenceSensors.IsOccupied()
            )
            .Subscribe(_ => MasterSwitch?.TurnOn());
    }

    private IDisposable TurnOnMotionSensorOnTvOff()
    {
        return MotionSensor
            .StateChanges()
            .IsOffForMinutes(30)
            .Where(_ => _entities.MediaPlayer.Tcl65c755.IsOff())
            .Subscribe(_ => MasterSwitch?.TurnOn());
    }

    private IDisposable SetSensorDelayOnKitchenOccupancy()
    {
        return _entities
            .BinarySensor.KitchenMotionSensors.StateChanges()
            .IsOnForSeconds(10)
            .Subscribe(_ => SensorDelay?.SetNumericValue(SensorActiveDelayValue));
    }

    private IDisposable TurnOffPantryLights()
    {
        return Light
            .StateChanges()
            .IsOff()
            .Where(_ => PantryUnoccupied())
            .Subscribe(_ => _entities.Light.PantryLights.TurnOff());
    }

    private bool PantryUnoccupied() =>
        _entities.Switch.PantryMotionSensor.IsOn() && _entities.BinarySensor.PantryMotionSensors.IsOff();

    public override void Dispose()
    {
        _dimmingController?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
