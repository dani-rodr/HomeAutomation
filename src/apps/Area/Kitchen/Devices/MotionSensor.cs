using HomeAutomation.apps.Common.Base;

namespace HomeAutomation.apps.Area.Kitchen.Devices;

public class MotionSensor(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    ILogger<MotionSensor> logger
) : Common.Base.MotionSensor(factory, scheduler, "kitchen_motion_sensor", logger);