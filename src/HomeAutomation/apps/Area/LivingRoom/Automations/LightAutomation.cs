using HomeAutomation.apps.Area.LivingRoom.Automations.Entities;
using HomeAutomation.apps.Area.LivingRoom.Config;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class LightAutomation(
    ILivingRoomLightEntities entities,
    LivingRoomLightSettings settings,
    IDimmingLightController dimmingController,
    ILogger<LightAutomation> logger
) : LightAutomationBase(entities, logger)
{
    private LivingRoomLightSettings Settings => settings;

    protected override int SensorWaitTime => Settings.SensorWaitSeconds;

    protected override int SensorActiveDelayValue => Settings.SensorActiveDelayValue;

    protected override int SensorInactiveDelayValue => Settings.SensorInactiveDelayValue;

    public override void StartAutomation()
    {
        base.StartAutomation();

        dimmingController.SetSensorActiveDelayValue(SensorActiveDelayValue);

        dimmingController.SetDimParameters(
            brightnessPct: Settings.DimmingBrightnessPct,
            delaySeconds: Settings.DimmingDelaySeconds
        );
    }

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return entities.LivingRoomDoor.OnOpened().Subscribe(TurnOnLights);

        yield return Light.OnTurnedOn().Subscribe(_ => entities.KitchenMotionAutomation.TurnOn());

        yield return MotionSensor.OnOccupied().Subscribe(TurnOnLights);

        yield return MotionSensor
            .OnCleared()
            .Subscribe(async _ => await dimmingController.OnMotionStoppedAsync(Light));
    }

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations()
    {
        yield return TurnOnMotionSensorOnTvOff();
    }

    protected override IEnumerable<IDisposable> GetAdditionalSwitchableAutomations()
    {
        yield return TurnOffPantryLights();

        yield return SetSensorDelayOnKitchenOccupancy();
    }

    private void TurnOnLights(StateChange e) => dimmingController.OnMotionDetected(Light);

    private IDisposable TurnOnMotionSensorOnTvOff()
    {
        return MotionSensor
            .OnCleared(new(Minutes: Settings.TvOffMasterSwitchReenableMinutes))
            .Where(_ => entities.TclTv.IsOff())
            .Subscribe(_ => MasterSwitch.TurnOn());
    }

    private IDisposable SetSensorDelayOnKitchenOccupancy()
    {
        return entities
            .KitchenMotionSensor.OnOccupied(new(Seconds: Settings.KitchenOccupancyDelaySeconds))
            .Subscribe(_ => SensorDelay?.SetNumericValue(SensorActiveDelayValue));
    }

    private IDisposable TurnOffPantryLights()
    {
        return Light
            .OnTurnedOff()
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
