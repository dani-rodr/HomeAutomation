namespace HomeAutomation.apps.Area.Bathroom.Automations;

public class MotionAutomation(Entities entities, ILogger<Bathroom> logger)
    : DimmingMotionAutomationBase(
        entities.Switch.BathroomMotionSensor,
        entities.BinarySensor.BathroomPresenceSensors,
        entities.Light.BathroomLights,
        entities.Number.ZEsp32C62StillTargetDelay,
        logger
    )
{
    protected override int DimBrightnessPct => 80;
    protected override int DimDelaySeconds => 5;
}
