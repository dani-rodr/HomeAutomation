using System.Collections.Generic;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class MotionAutomation(
    Entities entities,
    SwitchEntity masterSwitch,
    BinarySensorEntity motionSensor,
    ILogger logger
)
    : DimmingMotionAutomationBase(
        masterSwitch,
        motionSensor,
        entities.Light.SalaLightsGroup,
        logger,
        entities.Number.Ld2410Esp321StillTargetDelay
    )
{
    protected override int SensorWaitTime => 30;
    protected override int SensorActiveDelayValue => 45;
    protected override int SensorInactiveDelayValue => 1;
    protected override int DimBrightnessPct => 80;
    protected override int DimDelaySeconds => 15;

    protected override IEnumerable<IDisposable> GetAdditionalStartupAutomations()
    {
        yield return TurnOnMotionSensorOnTvOff(); // This is when we stay on sala while lights off then go to the room
    }

    protected override IEnumerable<IDisposable> GetAdditionalSwitchableAutomations()
    {
        yield return TurnOffPantryLights();
        yield return SetSensorDelayOnKitchenOccupancy();
    }

    private IDisposable TurnOnMotionSensorOnTvOff()
    {
        return MotionSensor
            .StateChanges()
            .IsOffForMinutes(30)
            .Where(_ => entities.MediaPlayer.Tcl65c755.IsOff())
            .Subscribe(_ => MasterSwitch?.TurnOn());
    }

    private IDisposable SetSensorDelayOnKitchenOccupancy()
    {
        return entities
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
            .Subscribe(_ => entities.Light.PantryLights.TurnOff());
    }

    private bool PantryUnoccupied() =>
        entities.Switch.PantryMotionSensor.IsOn() && entities.BinarySensor.PantryMotionSensors.IsOff();
}
