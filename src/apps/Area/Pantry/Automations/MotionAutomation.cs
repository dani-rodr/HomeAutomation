namespace HomeAutomation.apps.Area.Pantry.Automations;

/// <summary>
/// Pantry-specific motion automation with custom sensor timing values.
/// </summary>
public class MotionAutomation(Devices.MotionSensor motionSensor, ILogger<MotionAutomation> logger)
    : MotionAutomationBase(motionSensor, logger)
{
    public override int SensorWaitTime => 10;
    // ActiveDelayValue and InactiveDelayValue use defaults (5, 1)
}
