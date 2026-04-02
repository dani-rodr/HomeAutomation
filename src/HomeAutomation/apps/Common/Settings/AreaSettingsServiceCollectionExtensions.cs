using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Common.Settings;

public static class AreaSettingsServiceCollectionExtensions
{
    public static IServiceCollection AddAreaSettingsEngine(this IServiceCollection services)
    {
        return services
            .AddSingleton(_ =>
                AreaSettingsRegistry.CreateFromAssembly(
                    typeof(AreaSettingsServiceCollectionExtensions).Assembly
                )
            )
            .AddSingleton<IAreaSettingsChangeNotifier, AreaSettingsChangeNotifier>()
            .AddSingleton<IAreaSettingsStore, AreaSettingsStore>()
            .AddTransient(typeof(ILiveAppConfig<>), typeof(LiveAreaAppConfig<>));
    }
}
