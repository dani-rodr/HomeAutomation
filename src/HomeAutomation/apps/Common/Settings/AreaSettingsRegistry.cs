using System.IO;
using System.Linq;
using System.Reflection;

namespace HomeAutomation.apps.Common.Settings;

public sealed class AreaSettingsRegistry(IEnumerable<AreaSettingsDescriptor> descriptors)
{
    private readonly Dictionary<string, AreaSettingsDescriptor> _descriptors =
        descriptors.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<Type, AreaSettingsDescriptor> _descriptorsByType =
        descriptors.ToDictionary(x => x.SettingsType);

    public IReadOnlyCollection<AreaSettingsDescriptor> List() => _descriptors.Values.ToList();

    public bool TryGet(string areaKey, out AreaSettingsDescriptor descriptor) =>
        _descriptors.TryGetValue(areaKey, out descriptor!);

    public bool TryGetBySettingsType(Type settingsType, out AreaSettingsDescriptor descriptor) =>
        _descriptorsByType.TryGetValue(settingsType, out descriptor!);

    public static AreaSettingsRegistry CreateFromAssembly(Assembly assembly)
    {
        var descriptors = assembly
            .GetTypes()
            .Where(type =>
                type.IsClass
                && !type.IsAbstract
                && Attribute.IsDefined(type, typeof(AreaSettingsAttribute), inherit: false)
            )
            .Select(type =>
            {
                var attribute = type.GetCustomAttribute<AreaSettingsAttribute>(inherit: false)!;
                var folderName = ResolveAreaFolderName(type);
                var settingsFilePath = Path.Combine(
                    AppContext.BaseDirectory,
                    "apps",
                    "Area",
                    folderName,
                    "Config",
                    $"{attribute.Key}.settings.yaml"
                );

                return new AreaSettingsDescriptor(
                    Key: attribute.Key,
                    Name: attribute.Name,
                    Description: attribute.Description,
                    SettingsType: type,
                    SettingsFilePath: settingsFilePath
                );
            })
            .ToArray();

        return new AreaSettingsRegistry(descriptors);
    }

    private static string ResolveAreaFolderName(Type type)
    {
        var namespaceParts = type.Namespace?.Split('.') ?? [];
        var areaIndex = Array.FindIndex(
            namespaceParts,
            part => string.Equals(part, "Area", StringComparison.Ordinal)
        );

        if (areaIndex >= 0 && areaIndex + 1 < namespaceParts.Length)
        {
            return namespaceParts[areaIndex + 1];
        }

        throw new InvalidOperationException(
            $"Unable to resolve area folder from settings type namespace '{type.Namespace}'."
        );
    }
}
