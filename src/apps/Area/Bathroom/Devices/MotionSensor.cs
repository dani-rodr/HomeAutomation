using HomeAutomation.apps.Common.Base;

namespace HomeAutomation.apps.Area.Bathroom.Devices;

public class MotionSensor(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    ILogger<MotionSensor> logger
) : Common.Base.MotionSensor(factory, scheduler, "bathroom_motion_sensor", logger);