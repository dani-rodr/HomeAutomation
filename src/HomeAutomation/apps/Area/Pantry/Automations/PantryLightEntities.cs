using HomeAutomation.apps.Area.Pantry.Devices;

namespace HomeAutomation.apps.Area.Pantry.Automations;

public class PantryLightEntities(PantryDevices devices) : IPantryLightEntities
{
    public SwitchEntity MasterSwitch => devices.LightAutomation;
    public BinarySensorEntity MotionSensor => devices.MotionSensor;
    public LightEntity Light => devices.Lights;
    public NumberEntity SensorDelay => devices.SensorDelay;
    public ButtonEntity Restart => devices.Restart;
    public BinarySensorEntity MiScalePresenceSensor => devices.MiScalePresenceSensor;
    public LightEntity MirrorLight => devices.MirrorLight;
    public SwitchEntity BathroomMotionAutomation => devices.BathroomMotionAutomation;
    public BinarySensorEntity BathroomMotionSensor => devices.BathroomMotionSensor;
}
