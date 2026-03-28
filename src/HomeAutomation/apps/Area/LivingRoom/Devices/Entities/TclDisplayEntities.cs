namespace HomeAutomation.apps.Area.LivingRoom.Devices.Entities;

public class TclDisplayEntities(LivingRoomMediaDevices devices) : ITclDisplayEntities
{
    public MediaPlayerEntity MediaPlayer => devices.TclTv;
    public SwitchEntity MasterSwitch => devices.TvAutomation;
    public BinarySensorEntity MotionSensor => devices.MotionSensor;
    public NumberEntity SensorDelay => devices.SensorDelay;
    public LightEntity Light => devices.TvBacklight;
    public ButtonEntity Restart => devices.Restart;
}
