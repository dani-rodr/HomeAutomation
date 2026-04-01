using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HomeAutomation.apps.Common.Config;

public sealed class AreaConfigStore(
    IAreaConfigRegistry registry,
    IAreaConfigChangeNotifier changeNotifier,
    ILogger<AreaConfigStore> logger
) : IAreaConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private readonly object _sync = new();

    public IReadOnlyCollection<AreaConfigDescriptor> ListAreas() => registry.List();

    public JsonObject GetConfig(string areaKey)
    {
        var descriptor = ResolveDescriptor(areaKey);

        lock (_sync)
        {
            var defaults = LoadJsonObject(descriptor.DefaultsFilePath);
            if (!File.Exists(descriptor.OverridesFilePath))
            {
                return DeepClone(defaults);
            }

            try
            {
                var overrides = LoadJsonObject(descriptor.OverridesFilePath);
                var merged = DeepClone(defaults);
                MergeInto(merged, overrides);
                return merged;
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed reading area override from {OverridePath}. Falling back to defaults at {DefaultPath}",
                    descriptor.OverridesFilePath,
                    descriptor.DefaultsFilePath
                );
                return DeepClone(defaults);
            }
        }
    }

    public T GetConfig<T>(string areaKey)
        where T : class
    {
        var config = GetConfig(areaKey);
        var model = config.Deserialize<T>(JsonOptions);
        return model
            ?? throw new InvalidOperationException(
                $"Area config '{areaKey}' could not be deserialized to {typeof(T).Name}."
            );
    }

    public AreaConfigValidationResult SaveConfig(string areaKey, JsonObject config)
    {
        var descriptor = ResolveDescriptor(areaKey);
        var validation = ValidateAgainstSchema(descriptor, config);
        if (!validation.IsValid)
        {
            return validation;
        }

        lock (_sync)
        {
            PersistAtomically(descriptor.OverridesFilePath, config);
        }

        logger.LogInformation("Saved config override for area {AreaKey}", areaKey);
        changeNotifier.Publish(
            new AreaConfigChangedEvent(areaKey, AreaConfigChangeType.Saved, DateTimeOffset.UtcNow)
        );
        return AreaConfigValidationResult.Success;
    }

    public JsonObject ResetConfig(string areaKey)
    {
        var descriptor = ResolveDescriptor(areaKey);

        lock (_sync)
        {
            if (File.Exists(descriptor.OverridesFilePath))
            {
                File.Delete(descriptor.OverridesFilePath);
            }

            logger.LogInformation("Reset config override for area {AreaKey}", areaKey);
            changeNotifier.Publish(
                new AreaConfigChangedEvent(areaKey, AreaConfigChangeType.Reset, DateTimeOffset.UtcNow)
            );
            return LoadJsonObject(descriptor.DefaultsFilePath);
        }
    }

    private AreaConfigDescriptor ResolveDescriptor(string areaKey)
    {
        if (registry.TryGet(areaKey, out var descriptor))
        {
            return descriptor;
        }

        throw new KeyNotFoundException($"Unknown area config key: {areaKey}");
    }

    private static JsonObject LoadJsonObject(string path)
    {
        var json = File.ReadAllText(path);
        var node = JsonNode.Parse(json) as JsonObject;
        return node
            ?? throw new InvalidOperationException($"Config file at {path} is not a JSON object.");
    }

    private static void PersistAtomically(string path, JsonObject config)
    {
        var directory = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException("Area config path has no directory component.");
        }

        Directory.CreateDirectory(directory);
        var tempPath = Path.Combine(directory, $"{Path.GetFileName(path)}.tmp");
        File.WriteAllText(tempPath, config.ToJsonString(JsonOptions));
        File.Move(tempPath, path, overwrite: true);
    }

    private static JsonObject DeepClone(JsonObject source)
    {
        var clone = source.DeepClone() as JsonObject;
        return clone ?? throw new InvalidOperationException("Failed to clone JSON config object.");
    }

    private static void MergeInto(JsonObject target, JsonObject source)
    {
        foreach (var (key, value) in source)
        {
            if (value is JsonObject sourceObject)
            {
                if (target[key] is not JsonObject targetObject)
                {
                    targetObject = new JsonObject();
                    target[key] = targetObject;
                }

                MergeInto(targetObject, sourceObject);
                continue;
            }

            target[key] = value?.DeepClone();
        }
    }

    private AreaConfigValidationResult ValidateAgainstSchema(
        AreaConfigDescriptor descriptor,
        JsonObject config
    )
    {
        if (
            string.IsNullOrWhiteSpace(descriptor.SchemaFilePath)
            || !File.Exists(descriptor.SchemaFilePath)
        )
        {
            return AreaConfigValidationResult.Success;
        }

        AreaConfigSchema schema;
        try
        {
            var schemaJson = File.ReadAllText(descriptor.SchemaFilePath);
            schema = JsonSerializer.Deserialize<AreaConfigSchema>(schemaJson, JsonOptions) ?? new();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unable to read schema {SchemaPath} for area {AreaKey}.",
                descriptor.SchemaFilePath,
                descriptor.Key
            );
            return AreaConfigValidationResult.Failed(
                new Dictionary<string, string[]>
                {
                    ["schema"] = ["Area config schema is invalid and could not be loaded."],
                }
            );
        }

        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (path, rule) in GetExpandedFieldRules(schema))
        {
            var node = GetNodeByPath(config, path);
            if (node is null)
            {
                if (rule.Required)
                {
                    AddError(errors, path, "Value is required.");
                }
                continue;
            }

            ValidateField(path, node, rule, errors);
        }

        foreach (var comparison in GetExpandedComparisonRules(schema))
        {
            var left = GetNumericNodeValue(config, comparison.LeftPath);
            var right = GetNumericNodeValue(config, comparison.RightPath);
            if (!left.HasValue || !right.HasValue)
            {
                continue;
            }

            var isValid = comparison.Operator.ToLowerInvariant() switch
            {
                "lte" => left.Value <= right.Value,
                "gte" => left.Value >= right.Value,
                _ => true,
            };

            if (isValid)
            {
                continue;
            }

            var message = string.IsNullOrWhiteSpace(comparison.Message)
                ? $"Value must be {comparison.Operator} {comparison.RightPath}."
                : comparison.Message;
            AddError(errors, comparison.LeftPath, message);
        }

        if (errors.Count == 0)
        {
            return AreaConfigValidationResult.Success;
        }

        return AreaConfigValidationResult.Failed(
            errors.ToDictionary(static x => x.Key, static x => x.Value.ToArray())
        );
    }

    private static void ValidateField(
        string path,
        JsonNode node,
        AreaConfigFieldRule rule,
        IDictionary<string, List<string>> errors
    )
    {
        var normalizedType = rule.Type.ToLowerInvariant();

        switch (normalizedType)
        {
            case "integer":
            {
                if (!TryGetNumber(node, out var integerValue) || integerValue % 1 != 0)
                {
                    AddError(errors, path, "Value must be an integer.");
                    return;
                }

                ValidateNumberRange(path, integerValue, rule, errors);
                break;
            }
            case "number":
            {
                if (!TryGetNumber(node, out var numericValue))
                {
                    AddError(errors, path, "Value must be numeric.");
                    return;
                }

                ValidateNumberRange(path, numericValue, rule, errors);
                break;
            }
            case "boolean":
            {
                if (!(node is JsonValue booleanValue && booleanValue.TryGetValue<bool>(out _)))
                {
                    AddError(errors, path, "Value must be true/false.");
                }
                break;
            }
            case "string":
            {
                if (
                    !(
                        node is JsonValue stringValue
                        && stringValue.TryGetValue<string>(out var rawString)
                    )
                )
                {
                    AddError(errors, path, "Value must be a string.");
                    return;
                }

                if (
                    rule.AllowedValues.Length > 0
                    && !rule.AllowedValues.Contains(rawString, StringComparer.OrdinalIgnoreCase)
                )
                {
                    AddError(
                        errors,
                        path,
                        $"Value must be one of: {string.Join(", ", rule.AllowedValues)}."
                    );
                }
                break;
            }
        }
    }

    private static void ValidateNumberRange(
        string path,
        double value,
        AreaConfigFieldRule rule,
        IDictionary<string, List<string>> errors
    )
    {
        if (rule.Min.HasValue && value < rule.Min.Value)
        {
            AddError(errors, path, $"Value must be >= {rule.Min.Value}.");
        }

        if (rule.Max.HasValue && value > rule.Max.Value)
        {
            AddError(errors, path, $"Value must be <= {rule.Max.Value}.");
        }
    }

    private static JsonNode? GetNodeByPath(JsonObject root, string path)
    {
        JsonNode? current = root;
        var parts = path.Split(
            '.',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        foreach (var part in parts)
        {
            if (
                current is not JsonObject currentObject
                || !currentObject.TryGetPropertyValue(part, out current)
            )
            {
                return null;
            }
        }

        return current;
    }

    private static double? GetNumericNodeValue(JsonObject root, string path)
    {
        var node = GetNodeByPath(root, path);
        return node is not null && TryGetNumber(node, out var number) ? number : null;
    }

    private static bool TryGetNumber(JsonNode node, out double number)
    {
        number = 0;
        if (node is not JsonValue value)
        {
            return false;
        }

        if (value.TryGetValue<double>(out number))
        {
            return true;
        }

        if (value.TryGetValue<int>(out var intValue))
        {
            number = intValue;
            return true;
        }

        if (value.TryGetValue<long>(out var longValue))
        {
            number = longValue;
            return true;
        }

        return false;
    }

    private static void AddError(
        IDictionary<string, List<string>> errors,
        string key,
        string message
    )
    {
        if (!errors.TryGetValue(key, out var entry))
        {
            entry = [];
            errors[key] = entry;
        }

        entry.Add(message);
    }

    private static IEnumerable<KeyValuePair<string, AreaConfigFieldRule>> GetExpandedFieldRules(
        AreaConfigSchema schema
    )
    {
        foreach (var rule in schema.Fields)
        {
            yield return rule;
        }

        if (schema.Blocks.Length == 0 || schema.BlockFields.Count == 0)
        {
            yield break;
        }

        foreach (var block in schema.Blocks)
        {
            foreach (var (fieldName, rule) in schema.BlockFields)
            {
                yield return new KeyValuePair<string, AreaConfigFieldRule>(
                    $"{block}.{fieldName}",
                    rule
                );
            }
        }
    }

    private static IEnumerable<AreaConfigComparisonRule> GetExpandedComparisonRules(
        AreaConfigSchema schema
    )
    {
        foreach (var comparison in schema.Comparisons)
        {
            yield return comparison;
        }

        if (schema.Blocks.Length == 0 || schema.BlockComparisons.Count == 0)
        {
            yield break;
        }

        foreach (var block in schema.Blocks)
        {
            foreach (var comparison in schema.BlockComparisons)
            {
                yield return new AreaConfigComparisonRule
                {
                    LeftPath = $"{block}.{comparison.LeftField}",
                    RightPath = $"{block}.{comparison.RightField}",
                    Operator = comparison.Operator,
                    Message = comparison.Message,
                };
            }
        }
    }
}
