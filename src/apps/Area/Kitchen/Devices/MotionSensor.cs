namespace HomeAutomation.apps.Area.Kitchen.Devices;

public class MotionSensor(ITypedEntityFactory factory, ILogger logger)
    : MotionSensorBase(factory, "kitchen_motion_sensor", logger);
