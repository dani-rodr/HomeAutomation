using HomeAutomation.apps.Common.Base;

namespace HomeAutomation.apps.Area.Desk.Devices;

public class MotionSensor(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    ILogger<MotionSensor> logger
) : Common.Base.MotionSensor(factory, scheduler, "desk_motion_sensor", logger);