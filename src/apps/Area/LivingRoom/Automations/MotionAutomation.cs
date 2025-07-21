namespace HomeAutomation.apps.Area.LivingRoom.Automations;

/// <summary>
/// Living room-specific motion automation with custom sensor timing values.
/// </summary>
public class MotionAutomation(Devices.MotionSensor motionSensor, ILogger<MotionAutomation> logger)
    : MotionAutomationBase(motionSensor, logger)
{
    public override int SensorWaitTime => 30;
    public override int SensorActiveDelayValue => 45;
    public override int SensorInactiveDelayValue => 1;
}
