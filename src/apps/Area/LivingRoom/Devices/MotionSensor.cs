namespace HomeAutomation.apps.Area.LivingRoom.Devices;

public class MotionSensor(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    ILogger<MotionSensor> logger
) : MotionSensorBase(factory, scheduler, "sala_motion_sensor", logger);
