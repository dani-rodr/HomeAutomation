namespace HomeAutomation.apps.Area.Pantry.Devices;

public class MotionSensor(ITypedEntityFactory factory, ILogger logger)
    : MotionSensorBase(factory, "pantry_motion_sensor", logger);
