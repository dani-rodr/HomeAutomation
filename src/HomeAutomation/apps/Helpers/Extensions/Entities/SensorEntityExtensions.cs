using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class SensorEntityExtensions
{
    public static bool IsConnected([NotNullWhen(true)] this SensorEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.CONNECTED;

    public static bool IsDisconnected([NotNullWhen(true)] this SensorEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.DISCONNECTED;

    // This is different from an actual LockEntity
    public static bool IsLocked([NotNullWhen(true)] this SensorEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.LOCKED;

    public static bool IsUnlocked([NotNullWhen(true)] this SensorEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.UNLOCKED;

    public static int ToLocalHour(this SensorEntity sensor)
    {
        if (sensor.State is not string state || !DateTime.TryParse(state, out var utcTime))
        {
            return -1; // Use as fallback for invalid state
        }

        return utcTime.Hour;
    }
}
