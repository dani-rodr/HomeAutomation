namespace HomeAutomation.apps.Common.Config;

public sealed class AreaConfigSchema
{
    public string[] Blocks { get; init; } = [];

    public Dictionary<string, AreaConfigFieldRule> Fields { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, AreaConfigFieldRule> BlockFields { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public List<AreaConfigComparisonRule> Comparisons { get; init; } = [];

    public List<AreaConfigBlockComparisonRule> BlockComparisons { get; init; } = [];
}

public sealed class AreaConfigFieldRule
{
    public string Type { get; init; } = string.Empty;
    public bool Required { get; init; }
    public double? Min { get; init; }
    public double? Max { get; init; }
    public string[] AllowedValues { get; init; } = [];
}

public sealed class AreaConfigComparisonRule
{
    public string LeftPath { get; init; } = string.Empty;
    public string RightPath { get; init; } = string.Empty;
    public string Operator { get; init; } = string.Empty;
    public string? Message { get; init; }
}

public sealed class AreaConfigBlockComparisonRule
{
    public string LeftField { get; init; } = string.Empty;
    public string RightField { get; init; } = string.Empty;
    public string Operator { get; init; } = string.Empty;
    public string? Message { get; init; }
}
