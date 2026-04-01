namespace HomeAutomation.apps.Area.LivingRoom.Config;

public sealed class LivingRoomSettings
{
    public required LivingRoomAirQualitySettings AirQuality { get; init; }
    public required LivingRoomLightSettings Light { get; init; }
    public required LivingRoomFanSettings Fan { get; init; }
}

public sealed class LivingRoomAirQualitySettings
{
    public int CleanThresholdPm25 { get; init; } = 7;
    public int DirtyThresholdPm25 { get; init; } = 75;
    public int ManualOverrideResetMinutes { get; init; } = 10;
}

public sealed class LivingRoomLightSettings
{
    public int SensorWaitSeconds { get; init; } = 30;
    public int SensorActiveDelayValue { get; init; } = 45;
    public int SensorInactiveDelayValue { get; init; } = 1;
    public int DimmingBrightnessPct { get; init; } = 80;
    public int DimmingDelaySeconds { get; init; } = 15;
    public int TvOffMasterSwitchReenableMinutes { get; init; } = 30;
    public int KitchenOccupancyDelaySeconds { get; init; } = 10;
}

public sealed class LivingRoomFanSettings
{
    public int MotionOnDelaySeconds { get; init; } = 3;
    public int MotionOffDelayMinutes { get; init; } = 1;
}
