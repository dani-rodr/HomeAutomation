namespace HomeAutomation.apps.Area.Desk.Devices;

public class MotionSensor(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    ILogger<MotionSensor> logger
) : MotionSensorBase(factory, scheduler, "desk_motion_sensor", logger);
