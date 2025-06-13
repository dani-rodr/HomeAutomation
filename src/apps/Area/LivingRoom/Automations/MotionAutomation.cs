using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class MotionAutomation : MotionAutomationBase
{
    private readonly ILivingRoomMotionEntities _entities;
    private readonly DimmingLightController _dimmingController;

    protected override int SensorWaitTime => 30;
    protected override int SensorActiveDelayValue => 45;
    protected override int SensorInactiveDelayValue => 1;

    public MotionAutomation(ILivingRoomMotionEntities entities, ILogger logger)
        : base(entities.MasterSwitch, entities.MotionSensor, entities.Light, logger, entities.SensorDelay)
    {
        _entities = entities;
        _dimmingController = new DimmingLightController(
            SensorActiveDelayValue,
            entities.SensorDelay,
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
            .Where(_ => _entities.ContactSensorDoor.IsClosed() && _entities.BedroomPresenceSensors.IsOccupied())
            .Subscribe(_ => MasterSwitch?.TurnOn());
    }

    private IDisposable TurnOnMotionSensorOnTvOff()
    {
        return MotionSensor
            .StateChanges()
            .IsOffForMinutes(30)
            .Where(_ => _entities.TclTv.IsOff())
            .Subscribe(_ => MasterSwitch?.TurnOn());
    }

    private IDisposable SetSensorDelayOnKitchenOccupancy()
    {
        return _entities
            .KitchenMotionSensors.StateChanges()
            .IsOnForSeconds(10)
            .Subscribe(_ => SensorDelay?.SetNumericValue(SensorActiveDelayValue));
    }

    private IDisposable TurnOffPantryLights()
    {
        return Light
            .StateChanges()
            .IsOff()
            .Where(_ => PantryUnoccupied())
            .Subscribe(_ => _entities.PantryLights.TurnOff());
    }

    private bool PantryUnoccupied() => _entities.PantryMotionSensor.IsOn() && _entities.PantryMotionSensors.IsOff();

    public override void Dispose()
    {
        _dimmingController?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
