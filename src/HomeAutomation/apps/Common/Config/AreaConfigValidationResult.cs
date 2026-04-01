namespace HomeAutomation.apps.Common.Config;

public sealed record AreaConfigValidationResult(
    bool IsValid,
    IReadOnlyDictionary<string, string[]> Errors
)
{
    public static AreaConfigValidationResult Success { get; } =
        new(true, new Dictionary<string, string[]>());

    public static AreaConfigValidationResult Failed(IReadOnlyDictionary<string, string[]> errors) =>
        new(false, errors);
}
