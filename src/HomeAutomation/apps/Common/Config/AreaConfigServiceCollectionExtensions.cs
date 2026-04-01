using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Common.Config;

public static class AreaConfigServiceCollectionExtensions
{
    public static IServiceCollection AddAreaConfigEngine(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAreaConfigRegistry>(provider => new AreaConfigRegistry(
                provider.GetServices<AreaConfigDescriptor>()
            ))
            .AddSingleton<IAreaConfigChangeNotifier, AreaConfigChangeNotifier>()
            .AddSingleton<IAreaConfigStore, AreaConfigStore>();
    }

    public static IServiceCollection AddAreaConfig(
        this IServiceCollection services,
        string key,
        string name,
        string description,
        string? areaFolderName = null
    )
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Area key is required.", nameof(key));
        }

        var folderName = string.IsNullOrWhiteSpace(areaFolderName)
            ? char.ToUpperInvariant(key[0]) + key[1..]
            : areaFolderName;

        var configFolderPath = Path.Combine(
            AppContext.BaseDirectory,
            "apps",
            "Area",
            folderName,
            "Config"
        );

        return services.AddSingleton(
            new AreaConfigDescriptor(
                Key: key,
                Name: name,
                Description: description,
                DefaultsFilePath: Path.Combine(configFolderPath, $"{key}.config.json"),
                OverridesFilePath: Path.Combine(configFolderPath, $"{key}.config.local.json"),
                SchemaFilePath: Path.Combine(configFolderPath, $"{key}.config.schema.json")
            )
        );
    }
}
