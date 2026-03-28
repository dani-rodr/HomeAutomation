namespace HomeAutomation.apps.Area.Desk.Devices;

public class LaptopEntities(DeskDevices devices) : ILaptopEntities
{
    public SwitchEntity VirtualSwitch => devices.LaptopVirtualSwitch;
    public ButtonEntity WakeOnLanButton => devices.LaptopWakeOnLanButton;
    public SensorEntity Session => devices.LaptopSession;
    public NumericSensorEntity BatteryLevel => devices.LaptopBatteryLevel;
    public ButtonEntity Lock => devices.LaptopLock;
    public ButtonEntity Sleep => devices.LaptopSleep;
    public BinarySensorEntity MotionSensor => devices.MotionSensor;
}
