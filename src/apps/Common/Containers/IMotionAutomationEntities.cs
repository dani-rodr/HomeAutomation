namespace HomeAutomation.apps.Common.Containers;

public interface IMotionAutomationEntities
{
    SwitchEntity MasterSwitch { get; }
    BinarySensorEntity MotionSensor { get; }
    LightEntity Light { get; }
    NumberEntity SensorDelay { get; }
}

public interface IBedroomMotionEntities : IMotionAutomationEntities
{
    SwitchEntity RightSideEmptySwitch { get; }
    SwitchEntity LeftSideFanSwitch { get; }
}

public class BedroomMotionEntities(Entities entities) : IBedroomMotionEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.BedroomMotionSensor;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.BedroomPresenceSensors;
    public LightEntity Light => entities.Light.BedLights;
    public NumberEntity SensorDelay => entities.Number.Esp32PresenceBedroomStillTargetDelay;
    public SwitchEntity RightSideEmptySwitch => entities.Switch.Sonoff1002352c401;
    public SwitchEntity LeftSideFanSwitch => entities.Switch.Sonoff100238104e1;
}

public interface ILivingRoomMotionEntities : IMotionAutomationEntities
{
    BinarySensorEntity ContactSensorDoor { get; }
    BinarySensorEntity BedroomMotionSensors { get; }
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
    public BinarySensorEntity BedroomMotionSensors => entities.BinarySensor.BedroomPresenceSensors;
    public MediaPlayerEntity TclTv => entities.MediaPlayer.Tcl65c755;
    public BinarySensorEntity KitchenMotionSensors => entities.BinarySensor.KitchenMotionSensors;
    public LightEntity PantryLights => entities.Light.PantryLights;
    public SwitchEntity PantryMotionSensor => entities.Switch.PantryMotionSensor;
    public BinarySensorEntity PantryMotionSensors => entities.BinarySensor.PantryMotionSensors;
}

public interface IPantryMotionEntities : IMotionAutomationEntities
{
    BinarySensorEntity MiScalePresenceSensor { get; }
    LightEntity MirrorLight { get; }
    BinarySensorEntity RoomDoor { get; }
}

public class PantryMotionEntities(Entities entities) : IPantryMotionEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.PantryMotionSensor;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.PantryMotionSensors;
    public LightEntity Light => entities.Light.PantryLights;
    public NumberEntity SensorDelay => entities.Number.ZEsp32C63StillTargetDelay;
    public BinarySensorEntity MiScalePresenceSensor => entities.BinarySensor.Esp32PresenceBedroomMiScalePresence;
    public LightEntity MirrorLight => entities.Light.ControllerRgbDf1c0d;
    public BinarySensorEntity RoomDoor => entities.BinarySensor.ContactSensorDoor;
}

public interface IBathroomMotionEntities : IMotionAutomationEntities;

public class BathroomMotionEntities(Entities entities) : IBathroomMotionEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.BathroomMotionSensor;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.BathroomPresenceSensors;
    public LightEntity Light => entities.Light.BathroomLights;
    public NumberEntity SensorDelay => entities.Number.ZEsp32C62StillTargetDelay;
}
