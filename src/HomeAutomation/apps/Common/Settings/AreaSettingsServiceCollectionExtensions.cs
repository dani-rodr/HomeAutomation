using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Common.Settings;

public static class AreaSettingsServiceCollectionExtensions
{
    public static IServiceCollection AddAreaSettingsEngine(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAreaSettingsRegistry>(_ =>
                AreaSettingsRegistry.CreateFromAssembly(
                    typeof(AreaSettingsServiceCollectionExtensions).Assembly
                )
            )
            .AddSingleton<IAreaSettingsChangeNotifier, AreaSettingsChangeNotifier>()
            .AddSingleton<IAreaSettingsValidator, AreaSettingsValidator>()
            .AddSingleton<IAreaSettingsStore, AreaSettingsStore>();
    }
}
