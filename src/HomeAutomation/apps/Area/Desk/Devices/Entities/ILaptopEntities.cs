namespace HomeAutomation.apps.Area.Desk.Devices.Entities;

public interface ILaptopEntities
{
    SwitchEntity VirtualSwitch { get; }
    ButtonEntity WakeOnLanButton { get; }
    SensorEntity Session { get; }
    NumericSensorEntity BatteryLevel { get; }
    ButtonEntity Lock { get; }
    ButtonEntity Sleep { get; }
    BinarySensorEntity MotionSensor { get; }
}
