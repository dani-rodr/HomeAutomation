using System.Collections.Generic;

namespace HomeAutomation.apps.Helpers;

public static class HaEntityStates
{
    public const string ON = "on";
    public const string OFF = "off";
    public const string UNAVAILABLE = "unavailable";
    public const string UNKNOWN = "unknown";
    public const string DRY = "dry";
    public const string COOL = "cool";
    public const string AUTO = "auto";
}

public static class HaIdentity
{
    public const string ATHENA_BEZOS = "3c226ce9b1b9406495f0ecd486642611";
    public const string DANIEL_RODRIGUEZ = "7512fc7c361e45879df43f9f0f34fc57";
    public const string MIPAD5 = "b02831abf0e44536ad8fc552aede48c4";
    public const string SUPERVISOR = "f389ce79e38841e4bfd26c9685ffa784";
    public const string MANUAL = "";

    public static bool IsManuallyOperated(string? userId) =>
        string.IsNullOrEmpty(userId?.Trim()) || KnownUsers.Contains(userId.Trim());

    public static bool IsPhysicallyOperated(string? userId) => string.IsNullOrEmpty(userId);

    public static bool IsAutomated(string? userId) => userId is not null && (userId == SUPERVISOR);

    private static readonly HashSet<string> KnownUsers = [ATHENA_BEZOS, DANIEL_RODRIGUEZ, MIPAD5, MANUAL];
}
