namespace HomeAutomation.apps.Common.Settings;

public sealed record AreaSettingsValidationResult(
    bool IsValid,
    IReadOnlyDictionary<string, string[]> Errors
)
{
    public static AreaSettingsValidationResult Success { get; } =
        new(true, new Dictionary<string, string[]>());

    public static AreaSettingsValidationResult Failed(
        IReadOnlyDictionary<string, string[]> errors
    ) => new(false, errors);
}
