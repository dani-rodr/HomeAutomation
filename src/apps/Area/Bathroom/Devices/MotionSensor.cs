namespace HomeAutomation.apps.Area.Bathroom.Devices;

public class MotionSensor(ITypedEntityFactory factory, ILogger logger)
    : MotionSensorBase(factory, "bathroom_motion_sensor", logger);
