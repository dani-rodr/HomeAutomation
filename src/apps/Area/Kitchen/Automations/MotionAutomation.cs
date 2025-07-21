using HomeAutomation.apps.Common.Base;
using HomeAutomation.apps.Area.Kitchen.Devices;

namespace HomeAutomation.apps.Area.Kitchen.Automations;

/// <summary>
/// Kitchen-specific motion automation with custom sensor timing values.
/// </summary>
public class MotionAutomation(Devices.MotionSensor motionSensor, ILogger<MotionAutomation> logger)
    : MotionAutomationBase(motionSensor, logger)
{
    public override int SensorWaitTime => 30;
    public override int SensorActiveDelayValue => 60;
    public override int SensorInactiveDelayValue => 1;
}