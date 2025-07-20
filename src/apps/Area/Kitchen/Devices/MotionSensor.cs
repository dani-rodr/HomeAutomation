namespace HomeAutomation.apps.Area.Kitchen.Devices;

public class MotionSensor(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    ILogger<MotionSensor> logger
) : MotionSensorBase(factory, scheduler, "kitchen_motion_sensor", logger);
