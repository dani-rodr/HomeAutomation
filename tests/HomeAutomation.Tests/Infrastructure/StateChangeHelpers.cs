namespace HomeAutomation.Tests.Infrastructure;

/// <summary>
/// Helper utilities for creating StateChange objects in tests
/// </summary>
public static class StateChangeHelpers
{
    /// <summary>
    /// Creates a StateChange for an entity transitioning from one state to another
    /// </summary>
    public static StateChange CreateStateChange(IEntityCore entity, string oldState, string newState)
    {
        return new StateChange(
            (Entity)entity,
            new EntityState { State = oldState },
            new EntityState { State = newState }
        );
    }

    /// <summary>
    /// Creates a StateChange for a motion sensor turning on
    /// </summary>
    public static StateChange MotionDetected(BinarySensorEntity motionSensor) =>
        CreateStateChange(motionSensor, "off", "on");

    /// <summary>
    /// Creates a StateChange for a motion sensor turning off
    /// </summary>
    public static StateChange MotionCleared(BinarySensorEntity motionSensor) =>
        CreateStateChange(motionSensor, "on", "off");

    /// <summary>
    /// Creates a StateChange for a presence sensor detecting presence
    /// </summary>
    public static StateChange PresenceDetected(BinarySensorEntity presenceSensor) =>
        CreateStateChange(presenceSensor, "off", "on");

    /// <summary>
    /// Creates a StateChange for a presence sensor losing presence
    /// </summary>
    public static StateChange PresenceCleared(BinarySensorEntity presenceSensor) =>
        CreateStateChange(presenceSensor, "on", "off");

    /// <summary>
    /// Creates a StateChange for a door sensor opening
    /// </summary>
    public static StateChange DoorOpened(BinarySensorEntity doorSensor) => CreateStateChange(doorSensor, "off", "on");

    /// <summary>
    /// Creates a StateChange for a door sensor closing
    /// </summary>
    public static StateChange DoorClosed(BinarySensorEntity doorSensor) => CreateStateChange(doorSensor, "on", "off");

    /// <summary>
    /// Creates a StateChange for a switch turning on
    /// </summary>
    public static StateChange SwitchTurnedOn(SwitchEntity switchEntity) => CreateStateChange(switchEntity, "off", "on");

    /// <summary>
    /// Creates a StateChange for a switch turning off
    /// </summary>
    public static StateChange SwitchTurnedOff(SwitchEntity switchEntity) =>
        CreateStateChange(switchEntity, "on", "off");
}
