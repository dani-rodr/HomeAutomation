using HomeAutomation.apps.Common.Base;
using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk.Automations;

/// <summary>
/// Desk-specific motion automation using default sensor timing values.
/// </summary>
public class MotionAutomation(Devices.MotionSensor motionSensor, ILogger<MotionAutomation> logger)
    : MotionAutomationBase(motionSensor, logger)
{
    // Uses default timing values: WaitTime=15, ActiveDelay=5, InactiveDelay=1
}