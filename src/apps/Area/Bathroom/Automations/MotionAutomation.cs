using HomeAutomation.apps.Common.Base;
using HomeAutomation.apps.Area.Bathroom.Devices;

namespace HomeAutomation.apps.Area.Bathroom.Automations;

/// <summary>
/// Bathroom-specific motion automation using default sensor timing values.
/// </summary>
public class MotionAutomation(Devices.MotionSensor motionSensor, ILogger<MotionAutomation> logger)
    : MotionAutomationBase(motionSensor, logger)
{
    // Uses default timing values: WaitTime=15, ActiveDelay=5, InactiveDelay=1
}