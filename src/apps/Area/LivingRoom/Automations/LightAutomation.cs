namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class LightAutomation(
    ILivingRoomLightEntities entities,
    MotionAutomationBase motionAutomation,
    IDimmingLightController dimmingController,
    ILogger<LightAutomation> logger
) : LightAutomationBase(entities, motionAutomation, logger)
{
    public override void StartAutomation()
    {
        base.StartAutomation();

        dimmingController.SetSensorActiveDelayValue(MotionAutomation.SensorActiveDelayValue);
        dimmingController.SetDimParameters(brightnessPct: 80, delaySeconds: 15);
    }

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return entities.LivingRoomDoor.StateChanges().IsOpen().Subscribe(TurnOnLights);
        yield return MotionAutomation
            .GetMotionSensor()
            .StateChangesWithCurrent()
            .IsOn()
            .Subscribe(TurnOnLights);
        yield return MotionAutomation
            .GetMotionSensor()
            .StateChangesWithCurrent()
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

    private void TurnOnLights(StateChange e) => dimmingController.OnMotionDetected(Light);

    private IDisposable TurnOnMotionSensorAfterNoMotionAndRoomOccupied()
    {
        return MotionAutomation
            .GetMotionSensor()
            .StateChanges()
            .IsOffForMinutes(2)
            .Where(_ =>
                entities.BedroomDoor.IsClosed() && entities.BedroomMotionSensor.IsOccupied()
            )
            .Subscribe(_ => MasterSwitch.TurnOn());
    }

    private IDisposable TurnOnMotionSensorOnTvOff()
    {
        return MotionAutomation
            .GetMotionSensor()
            .StateChanges()
            .IsOffForMinutes(30)
            .Where(_ => entities.TclTv.IsOff())
            .Subscribe(_ => MasterSwitch.TurnOn());
    }

    private IDisposable SetSensorDelayOnKitchenOccupancy()
    {
        return entities
            .KitchenMotionSensor.StateChanges()
            .IsOnForSeconds(10)
            .Subscribe(_ =>
                MotionAutomation
                    .GetSensorDelay()
                    ?.SetNumericValue(MotionAutomation.SensorActiveDelayValue)
            );
    }

    private IDisposable TurnOffPantryLights()
    {
        return Light
            .StateChanges()
            .IsOff()
            .Where(_ => PantryUnoccupied())
            .Subscribe(_ => entities.PantryLights.TurnOff());
    }

    private bool PantryUnoccupied() =>
        entities.PantryMotionAutomation.IsOn() && entities.PantryMotionSensor.IsOff();

    public override void Dispose()
    {
        dimmingController?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
