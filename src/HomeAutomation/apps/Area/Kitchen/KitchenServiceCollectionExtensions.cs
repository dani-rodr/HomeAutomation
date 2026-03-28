using HomeAutomation.apps.Area.Kitchen.Automations;
using HomeAutomation.apps.Area.Kitchen.Devices;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Area.Kitchen;

public static class KitchenServiceCollectionExtensions
{
    public static IServiceCollection AddKitchenServices(this IServiceCollection services)
    {
        return services
            .AddTransient<KitchenDevices>()
            .AddTransient<IKitchenLightEntities, KitchenLightEntities>()
            .AddTransient<ICookingEntities, KitchenCookingEntities>();
    }
}
