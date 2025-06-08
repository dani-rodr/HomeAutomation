using System.Collections.Generic;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class MotionAutomation(Entities entities, ILogger<LivingRoom> logger)
    : DimmingMotionAutomationBase(
        entities.Switch.SalaMotionSensor,
        entities.BinarySensor.LivingRoomPresenceSensors,
        entities.Light.SalaLightsGroup,
        entities.Number.Ld2410Esp321StillTargetDelay,
        logger
    )
{
    protected override int SensorWaitTime => 30;
    protected override int SensorDelayValueActive => 45;
    protected override int SensorDelayValueInactive => 1;
    protected override int DimBrightnessPct => 80;
    protected override int DimDelaySeconds => 15;
    private readonly BinarySensorEntity livingRoomMotionSensor = entities.BinarySensor.Ld2410Esp321SmartPresence;

    public override void StartAutomation()
    {
        base.StartAutomation();
    }

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        bool turnOffPantryLights =
            entities.Switch.PantryMotionSensor.IsOn() && entities.BinarySensor.PantryMotionSensors.IsOff();
        return [.. base.GetLightAutomations(), AutoPantryOffWithSala()];
    }

    private IDisposable AutoPantryOffWithSala()
    {
        return Light
            .StateChanges()
            .IsOff()
            .Where(_ => ShouldPantryLightsTurnOff())
            .Subscribe(_ => entities.Light.PantryLights.TurnOff());
    }

    private bool ShouldPantryLightsTurnOff() =>
        entities.Switch.PantryMotionSensor.IsOn() && entities.BinarySensor.PantryMotionSensors.IsOff();
}
