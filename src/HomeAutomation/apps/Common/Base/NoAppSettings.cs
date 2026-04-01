namespace HomeAutomation.apps.Common.Base;

public sealed class NoAppSettings
{
    public static NoAppSettings Instance { get; } = new();

    private NoAppSettings() { }
}
