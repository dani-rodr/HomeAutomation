namespace HomeAutomation.apps.Area.Bathroom.Automations;

public class MotionAutomation(Entities entities, ILogger logger)
    : DimmingMotionAutomationBase(
        entities.Switch.BathroomMotionSensor,
        entities.BinarySensor.BathroomPresenceSensors,
        entities.Light.BathroomLights,
        logger,
        entities.Number.ZEsp32C62StillTargetDelay
    )
{
    protected override int DimBrightnessPct => 80;
    protected override int DimDelaySeconds => 5;
}
