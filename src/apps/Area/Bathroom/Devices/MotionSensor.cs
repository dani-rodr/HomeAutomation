namespace HomeAutomation.apps.Area.Bathroom.Devices;

public class MotionSensor(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    ILogger<MotionSensor> logger
) : MotionSensorBase(factory, scheduler, "bathroom_motion_sensor", logger);
