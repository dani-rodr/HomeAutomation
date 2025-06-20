namespace HomeAutomation.Tests.Infrastructure;

/// <summary>
/// Helper utilities for creating StateChange objects in tests
/// </summary>
public static class StateChangeHelpers
{
    /// <summary>
    /// Creates a StateChange for an entity transitioning from one state to another
    /// </summary>
    public static StateChange CreateStateChange(
        IEntityCore entity,
        string oldState,
        string newState
    )
    {
        return new StateChange(
            (Entity)entity,
            new EntityState { State = oldState },
            new EntityState { State = newState }
        );
    }

    /// <summary>
    /// Creates a StateChange for an entity transitioning from one state to another with user context
    /// </summary>
    public static StateChange CreateStateChange(
        IEntityCore entity,
        string oldState,
        string newState,
        string? userId
    )
    {
        return new StateChange(
            (Entity)entity,
            new EntityState { State = oldState },
            new EntityState
            {
                State = newState,
                Context = new Context { UserId = userId },
            }
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
    public static StateChange DoorOpened(BinarySensorEntity doorSensor) =>
        CreateStateChange(doorSensor, "off", "on");

    /// <summary>
    /// Creates a StateChange for a door sensor closing
    /// </summary>
    public static StateChange DoorClosed(BinarySensorEntity doorSensor) =>
        CreateStateChange(doorSensor, "on", "off");

    /// <summary>
    /// Creates a StateChange for a switch turning on
    /// </summary>
    public static StateChange SwitchTurnedOn(SwitchEntity switchEntity) =>
        CreateStateChange(switchEntity, "off", "on");

    /// <summary>
    /// Creates a StateChange for a switch turning off
    /// </summary>
    public static StateChange SwitchTurnedOff(SwitchEntity switchEntity) =>
        CreateStateChange(switchEntity, "on", "off");

    /// <summary>
    /// Creates a StateChange for a switch with user context
    /// </summary>
    public static StateChange CreateSwitchStateChange(
        SwitchEntity switchEntity,
        string oldState,
        string newState,
        string? userId
    ) => CreateStateChange(switchEntity, oldState, newState, userId);

    /// <summary>
    /// Creates a StateChange for a light with user context
    /// </summary>
    public static StateChange CreateLightStateChange(
        LightEntity lightEntity,
        string oldState,
        string newState,
        string? userId
    ) => CreateStateChange(lightEntity, oldState, newState, userId);

    /// <summary>
    /// Creates a StateChange for a climate entity with user context
    /// </summary>
    public static StateChange CreateClimateStateChange(
        ClimateEntity climateEntity,
        string oldState,
        string newState,
        string? userId
    ) => CreateStateChange(climateEntity, oldState, newState, userId);

    /// <summary>
    /// Creates a StateChange for a weather entity
    /// </summary>
    public static StateChange CreateWeatherStateChange(
        WeatherEntity weatherEntity,
        string oldState,
        string newState
    ) => CreateStateChange(weatherEntity, oldState, newState);

    /// <summary>
    /// Creates a StateChange for an input boolean entity
    /// </summary>
    public static StateChange CreateInputBooleanStateChange(
        InputBooleanEntity inputBoolean,
        string oldState,
        string newState
    ) => CreateStateChange(inputBoolean, oldState, newState);

    /// <summary>
    /// Creates a StateChange for a button press
    /// </summary>
    public static StateChange CreateButtonPress(ButtonEntity button) =>
        CreateStateChange(button, "", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

    /// <summary>
    /// Creates a StateChange for a button press with user context
    /// </summary>
    public static StateChange CreateButtonPress(ButtonEntity button, string? userId) =>
        CreateStateChange(button, "", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), userId);

    /// <summary>
    /// Creates a StateChange for an input button press
    /// </summary>
    public static StateChange CreateButtonPress(InputButtonEntity button) =>
        CreateStateChange(button, "", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

    /// <summary>
    /// Creates a StateChange for an input button press with user context
    /// </summary>
    public static StateChange CreateButtonPress(InputButtonEntity button, string? userId) =>
        CreateStateChange(button, "", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), userId);

    /// <summary>
    /// Creates a StateChange for a lock being locked
    /// </summary>
    public static StateChange LockLocked(LockEntity lockEntity) =>
        CreateStateChange(lockEntity, "unlocked", "locked");

    /// <summary>
    /// Creates a StateChange for a lock being unlocked
    /// </summary>
    public static StateChange LockUnlocked(LockEntity lockEntity) =>
        CreateStateChange(lockEntity, "locked", "unlocked");

    /// <summary>
    /// Creates a StateChange for a lock with user context
    /// </summary>
    public static StateChange CreateLockStateChange(
        LockEntity lockEntity,
        string oldState,
        string newState,
        string? userId
    ) => CreateStateChange(lockEntity, oldState, newState, userId);

    /// <summary>
    /// Creates a StateChange for a numeric sensor transitioning from one value to another
    /// </summary>
    public static StateChange CreateNumericSensorStateChange(
        NumericSensorEntity sensor,
        string oldValue,
        string newValue
    ) => CreateStateChange(sensor, oldValue, newValue);
}
