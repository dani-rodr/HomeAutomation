using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class SensorEntityExtensions
{
    public static bool IsConnected([NotNullWhen(true)] this SensorEntity? entity) =>
        entity?.State is HaEntityStates.CONNECTED;

    public static bool IsDisconnected([NotNullWhen(true)] this SensorEntity? entity) =>
        entity?.State is HaEntityStates.DISCONNECTED;

    public static int ToLocalHour(this SensorEntity sensor)
    {
        if (sensor.State is not string state || !DateTime.TryParse(state, out var utcTime))
        {
            return -1; // Use as fallback for invalid state
        }

        return utcTime.Hour;
    }
}
