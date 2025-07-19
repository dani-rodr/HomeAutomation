namespace HomeAutomation.apps.Area.Bedroom.Devices;

public class MotionSensor(ITypedEntityFactory factory, ILogger logger)
    : MotionSensorBase(factory, "bedroom_motion_sensor", logger);
