namespace HomeAutomation.apps.Area.Desk.Devices;

public class MotionSensor(ITypedEntityFactory factory, ILogger logger)
    : MotionSensorBase(factory, "desk_motion_sensor", logger);
