namespace HomeAutomation.apps.Common.Containers;

public class Devices(Entities entities)
{
    public MotionSensors MotionSensors { get; } = new(entities);
}

public class MotionSensors(Entities entities)
{
    public MotionSensor Bedroom { get; } =
        new(
            entities.BinarySensor.BedroomPresenceSensors,
            entities.Switch.BedroomMotionSensor,
            entities.Button.Esp32PresenceBedroomRestartEsp32,
            entities.Number.Esp32PresenceBedroomStillTargetDelay
        );
    public MotionSensor LivingRoom { get; } =
        new(
            entities.BinarySensor.LivingRoomPresenceSensors,
            entities.Switch.SalaMotionSensor,
            entities.Button.Ld2410Esp321RestartEsp32,
            entities.Number.Ld2410Esp321StillTargetDelay
        );
    public MotionSensor Kitchen { get; } =
        new(
            entities.BinarySensor.KitchenMotionSensors,
            entities.Switch.KitchenMotionSensor,
            entities.Button.Ld2410Esp325RestartEsp32,
            entities.Number.Ld2410Esp325StillTargetDelay
        );
    public MotionSensor Pantry { get; } =
        new(
            entities.BinarySensor.PantryMotionSensors,
            entities.Switch.PantryMotionSensor,
            entities.Button.ZEsp32C63RestartEsp32,
            entities.Number.ZEsp32C63StillTargetDelay
        );
    public MotionSensor Desk { get; } =
        new(
            entities.BinarySensor.DeskSmartPresence,
            entities.Switch.LgTvMotionSensor,
            entities.Button.ZEsp32C61RestartEsp322,
            entities.Number.ZEsp32C61StillTargetDelay2
        );
    public MotionSensor Bathroom { get; } =
        new(
            entities.BinarySensor.BathroomPresenceSensors,
            entities.Switch.BathroomMotionSensor,
            entities.Button.ZEsp32C62RestartEsp32,
            entities.Number.ZEsp32C62StillTargetDelay
        );

    public IEnumerable<MotionSensor> All => [Bedroom, LivingRoom, Kitchen, Pantry, Desk, Bathroom];

    public record MotionSensor(
        BinarySensorEntity Sensor,
        SwitchEntity Automation,
        ButtonEntity Restart,
        NumberEntity Timer
    );
}
