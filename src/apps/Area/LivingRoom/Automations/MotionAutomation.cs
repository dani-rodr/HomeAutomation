namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class MotionAutomation(
    ILivingRoomMotionEntities entities,
    IDimmingLightController dimmingController,
    ILogger logger
) : MotionAutomationBase(entities.MasterSwitch, entities.MotionSensor, entities.Light, logger, entities.SensorDelay)
{
    protected override int SensorWaitTime => 30;
    protected override int SensorActiveDelayValue => 45;
    protected override int SensorInactiveDelayValue => 1;

    public override void StartAutomation()
    {
        base.StartAutomation();

        dimmingController.SetSensorActiveDelayValue(SensorActiveDelayValue);
        dimmingController.SetDimParameters(brightnessPct: 80, delaySeconds: 15);
    }

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor.StateChanges().IsOn().Subscribe(e => dimmingController.OnMotionDetected(Light));
        yield return MotionSensor
            .StateChanges()
            .IsOff()
            .Subscribe(async _ => await dimmingController.OnMotionStoppedAsync(Light));
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
            .Where(_ => entities.ContactSensorDoor.IsClosed() && entities.BedroomMotionSensors.IsOccupied())
            .Subscribe(_ => MasterSwitch?.TurnOn());
    }

    private IDisposable TurnOnMotionSensorOnTvOff()
    {
        return MotionSensor
            .StateChanges()
            .IsOffForMinutes(30)
            .Where(_ => entities.TclTv.IsOff())
            .Subscribe(_ => MasterSwitch?.TurnOn());
    }

    private IDisposable SetSensorDelayOnKitchenOccupancy()
    {
        return entities
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
            .Subscribe(_ => entities.PantryLights.TurnOff());
    }

    private bool PantryUnoccupied() => entities.PantryMotionSensor.IsOn() && entities.PantryMotionSensors.IsOff();

    public override void Dispose()
    {
        dimmingController?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
