using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HomeAutomation.apps.Common.Settings;

public sealed class AreaSettingsStore(
    IAreaSettingsRegistry registry,
    IAreaSettingsValidator validator,
    IAreaSettingsChangeNotifier changeNotifier,
    ILogger<AreaSettingsStore> logger
) : IAreaSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithAttemptingUnquotedStringTypeDeserialization()
        .Build();

    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private readonly Dictionary<string, JsonObject> _bootDefaults = registry
        .List()
        .ToDictionary(
            descriptor => descriptor.Key,
            descriptor => LoadSectionFromFile(descriptor),
            StringComparer.OrdinalIgnoreCase
        );

    public IReadOnlyCollection<AreaSettingsDescriptor> ListAreas() => registry.List();

    public JsonObject GetSettings(string areaKey)
    {
        var descriptor = ResolveDescriptor(areaKey);
        return LoadSectionFromFile(descriptor);
    }

    public T GetSettings<T>(string areaKey)
        where T : class
    {
        return (T)GetSettings(areaKey, typeof(T));
    }

    public object GetSettings(string areaKey, Type settingsType)
    {
        ArgumentNullException.ThrowIfNull(settingsType);
        var descriptor = ResolveDescriptor(areaKey);

        if (descriptor.SettingsType != settingsType)
        {
            throw new InvalidOperationException(
                $"Area '{areaKey}' requires settings type '{descriptor.SettingsType.Name}', but '{settingsType.Name}' was requested."
            );
        }

        var section = LoadSectionFromFile(descriptor);
        return DeserializeToType(section, settingsType);
    }

    public AreaSettingsValidationResult SaveSettings(string areaKey, JsonObject settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var descriptor = ResolveDescriptor(areaKey);

        object typed;
        try
        {
            typed = DeserializeToType(settings, descriptor.SettingsType);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to deserialize settings for area '{AreaKey}'.", areaKey);
            return AreaSettingsValidationResult.Failed(
                new Dictionary<string, string[]>
                {
                    ["settings"] = ["Settings format is invalid for this area."],
                }
            );
        }

        var errors = validator.Validate(typed);
        if (errors.Count > 0)
        {
            return AreaSettingsValidationResult.Failed(errors);
        }

        SaveSectionToFile(descriptor, settings);

        changeNotifier.Publish(
            new AreaSettingsChangedEvent(
                areaKey,
                AreaSettingsChangeType.Saved,
                DateTimeOffset.UtcNow
            )
        );

        return AreaSettingsValidationResult.Success;
    }

    public JsonObject ResetSettings(string areaKey)
    {
        var descriptor = ResolveDescriptor(areaKey);
        var defaultSettings = _bootDefaults[descriptor.Key].DeepClone().AsObject();

        SaveSectionToFile(descriptor, defaultSettings);

        changeNotifier.Publish(
            new AreaSettingsChangedEvent(
                areaKey,
                AreaSettingsChangeType.Reset,
                DateTimeOffset.UtcNow
            )
        );

        return defaultSettings;
    }

    private AreaSettingsDescriptor ResolveDescriptor(string areaKey)
    {
        if (!registry.TryGet(areaKey, out var descriptor))
        {
            throw new KeyNotFoundException($"Unknown area '{areaKey}'.");
        }

        return descriptor;
    }

    private static JsonObject LoadSectionFromFile(AreaSettingsDescriptor descriptor)
    {
        if (!File.Exists(descriptor.SettingsFilePath))
        {
            throw new FileNotFoundException(
                $"Settings file not found: '{descriptor.SettingsFilePath}'.",
                descriptor.SettingsFilePath
            );
        }

        var root = LoadYamlAsJsonObject(descriptor.SettingsFilePath);
        if (
            root[descriptor.SettingsSectionKey] is not JsonObject section
            && root[ToCamelCase(descriptor.SettingsSectionKey)] is not JsonObject
        )
        {
            throw new InvalidOperationException(
                $"Settings file '{descriptor.SettingsFilePath}' does not contain section '{descriptor.SettingsSectionKey}'."
            );
        }

        section =
            root[descriptor.SettingsSectionKey] as JsonObject
            ?? (root[ToCamelCase(descriptor.SettingsSectionKey)] as JsonObject)!;

        return section.DeepClone().AsObject();
    }

    private static void SaveSectionToFile(AreaSettingsDescriptor descriptor, JsonObject settings)
    {
        var root = new JsonObject { [descriptor.SettingsSectionKey] = settings.DeepClone() };

        var yaml = YamlSerializer.Serialize(ToPlainObject(root));
        SaveAtomically(descriptor.SettingsFilePath, yaml);
    }

    private static JsonObject LoadYamlAsJsonObject(string filePath)
    {
        var yaml = File.ReadAllText(filePath);
        var raw = YamlDeserializer.Deserialize<object?>(yaml);
        var node = ToJsonNode(raw);
        return node as JsonObject
            ?? throw new InvalidOperationException(
                $"Settings YAML '{filePath}' must contain an object."
            );
    }

    private static object DeserializeToType(JsonObject settings, Type settingsType)
    {
        var typed = settings.Deserialize(settingsType, JsonOptions);
        return typed
            ?? throw new InvalidOperationException(
                $"Unable to deserialize settings to '{settingsType.Name}'."
            );
    }

    private static void SaveAtomically(string filePath, string content)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException($"Cannot resolve directory for '{filePath}'.");
        }

        Directory.CreateDirectory(directory);

        var tempPath = Path.Combine(
            directory,
            $".{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp"
        );
        File.WriteAllText(tempPath, content);

        if (File.Exists(filePath))
        {
            File.Replace(
                tempPath,
                filePath,
                destinationBackupFileName: null,
                ignoreMetadataErrors: true
            );
            return;
        }

        File.Move(tempPath, filePath);
    }

    private static JsonNode? ToJsonNode(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is IDictionary<object, object> objectDictionary)
        {
            var obj = new JsonObject();
            foreach (var entry in objectDictionary)
            {
                var key = entry.Key?.ToString();
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                obj[key] = ToJsonNode(entry.Value);
            }

            return obj;
        }

        if (value is IDictionary<string, object> stringDictionary)
        {
            var obj = new JsonObject();
            foreach (var entry in stringDictionary)
            {
                obj[entry.Key] = ToJsonNode(entry.Value);
            }

            return obj;
        }

        if (value is IEnumerable<object> listValue && value is not string)
        {
            var array = new JsonArray();
            foreach (var item in listValue)
            {
                array.Add(ToJsonNode(item));
            }

            return array;
        }

        return JsonSerializer.SerializeToNode(value, JsonOptions);
    }

    private static object? ToPlainObject(JsonNode? node)
    {
        return node switch
        {
            null => null,
            JsonValue value => ToPlainScalar(value),
            JsonArray array => array.Select(ToPlainObject).ToList(),
            JsonObject obj => obj.ToDictionary(x => x.Key, x => ToPlainObject(x.Value)),
            _ => null,
        };
    }

    private static object? ToPlainScalar(JsonValue value)
    {
        if (value.TryGetValue<bool>(out var asBool))
        {
            return asBool;
        }

        if (value.TryGetValue<int>(out var asInt))
        {
            return asInt;
        }

        if (value.TryGetValue<long>(out var asLong))
        {
            return asLong;
        }

        if (value.TryGetValue<double>(out var asDouble))
        {
            return asDouble;
        }

        if (value.TryGetValue<decimal>(out var asDecimal))
        {
            return asDecimal;
        }

        if (value.TryGetValue<string>(out var asString))
        {
            return asString;
        }

        return value.ToString();
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
