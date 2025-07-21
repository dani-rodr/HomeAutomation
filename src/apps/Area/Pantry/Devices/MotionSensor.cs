namespace HomeAutomation.apps.Area.Pantry.Devices;

public class MotionSensor(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    ILogger<MotionSensor> logger
) : Common.Base.MotionSensor(factory, scheduler, "pantry_motion_sensor", logger);
