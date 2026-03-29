using HomeAutomation.apps.Area.LivingRoom.Devices;

namespace HomeAutomation.apps.Area.LivingRoom.Automations.Entities;

public class LightEntities(LivingRoomLightDevices devices, LivingRoomMediaDevices mediaDevices)
    : ILivingRoomLightEntities
{
    public SwitchEntity MasterSwitch => devices.LightAutomation;
    public BinarySensorEntity MotionSensor => devices.MotionSensor;
    public LightEntity Light => devices.Lights;
    public NumberEntity SensorDelay => devices.SensorDelay;
    public ButtonEntity Restart => devices.Restart;
    public BinarySensorEntity BedroomDoor => devices.BedroomDoor;
    public BinarySensorEntity LivingRoomDoor => devices.LivingRoomDoor;
    public BinarySensorEntity BedroomMotionSensor => devices.BedroomMotionSensor;
    public MediaPlayerEntity TclTv => mediaDevices.TclTv;
    public BinarySensorEntity KitchenMotionSensor => devices.KitchenMotionSensor;
    public SwitchEntity KitchenMotionAutomation => devices.KitchenMotionAutomation;
    public LightEntity PantryLights => devices.PantryLights;
    public SwitchEntity PantryMotionAutomation => devices.PantryMotionAutomation;
    public BinarySensorEntity PantryMotionSensor => devices.PantryMotionSensor;
}
