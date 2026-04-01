namespace HomeAutomation.apps.Area.Pantry.Config;

public sealed class PantrySettings
{
    public required PantryLightSettings Light { get; init; }
}

public sealed class PantryLightSettings
{
    public int SensorWaitSeconds { get; init; } = 5;
    public int SensorActiveDelayValue { get; init; } = 5;
    public int BathroomAutomationTurnOffDelaySeconds { get; init; } = 60;
}
