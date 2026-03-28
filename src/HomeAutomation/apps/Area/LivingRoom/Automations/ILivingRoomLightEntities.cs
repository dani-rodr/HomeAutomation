namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public interface ILivingRoomLightEntities : ILightAutomationEntities
{
    BinarySensorEntity BedroomDoor { get; }
    BinarySensorEntity LivingRoomDoor { get; }
    BinarySensorEntity BedroomMotionSensor { get; }
    MediaPlayerEntity TclTv { get; }
    BinarySensorEntity KitchenMotionSensor { get; }
    SwitchEntity KitchenMotionAutomation { get; }
    LightEntity PantryLights { get; }
    SwitchEntity PantryMotionAutomation { get; }
    BinarySensorEntity PantryMotionSensor { get; }
}
