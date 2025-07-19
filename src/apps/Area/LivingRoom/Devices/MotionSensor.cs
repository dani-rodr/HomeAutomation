namespace HomeAutomation.apps.Area.LivingRoom.Devices;

public class MotionSensor(ITypedEntityFactory factory, ILogger<MotionSensor> logger)
    : MotionSensorBase(factory, "sala_motion_sensor", logger);
