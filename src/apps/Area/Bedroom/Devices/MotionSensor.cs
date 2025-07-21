using HomeAutomation.apps.Common.Base;

namespace HomeAutomation.apps.Area.Bedroom.Devices;

public class MotionSensor(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    ILogger<MotionSensor> logger
) : Common.Base.MotionSensor(factory, scheduler, "bedroom_motion_sensor", logger);