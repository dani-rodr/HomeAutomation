namespace HomeAutomation.apps.Common.Containers;

public interface ILivingRoomMotionEntities : IMotionAutomationEntities
{
    BinarySensorEntity ContactSensorDoor { get; }
    BinarySensorEntity BedroomPresenceSensors { get; }
    MediaPlayerEntity TclTv { get; }
    BinarySensorEntity KitchenMotionSensors { get; }
    LightEntity PantryLights { get; }
    SwitchEntity PantryMotionSensor { get; }
    BinarySensorEntity PantryMotionSensors { get; }
}

public class LivingRoomMotionEntities(Entities entities) : ILivingRoomMotionEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.SalaMotionSensor;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.LivingRoomPresenceSensors;
    public LightEntity Light => entities.Light.SalaLightsGroup;
    public NumberEntity SensorDelay => entities.Number.Ld2410Esp321StillTargetDelay;
    public BinarySensorEntity ContactSensorDoor => entities.BinarySensor.ContactSensorDoor;
    public BinarySensorEntity BedroomPresenceSensors => entities.BinarySensor.BedroomPresenceSensors;
    public MediaPlayerEntity TclTv => entities.MediaPlayer.Tcl65c755;
    public BinarySensorEntity KitchenMotionSensors => entities.BinarySensor.KitchenMotionSensors;
    public LightEntity PantryLights => entities.Light.PantryLights;
    public SwitchEntity PantryMotionSensor => entities.Switch.PantryMotionSensor;
    public BinarySensorEntity PantryMotionSensors => entities.BinarySensor.PantryMotionSensors;
}
