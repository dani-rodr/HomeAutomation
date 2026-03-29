using HomeAutomation.apps.Area.Kitchen.Automations.Entities;
using HomeAutomation.apps.Area.Kitchen.Devices;
using Microsoft.Extensions.DependencyInjection;
using KitchenCookingEntityAdapter = HomeAutomation.apps.Area.Kitchen.Automations.Entities.CookingEntities;
using KitchenLightEntityAdapter = HomeAutomation.apps.Area.Kitchen.Automations.Entities.LightEntities;

namespace HomeAutomation.apps.Area.Kitchen;

public static class KitchenServiceCollectionExtensions
{
    public static IServiceCollection AddKitchenServices(this IServiceCollection services)
    {
        return services
            .AddTransient<KitchenDevices>()
            .AddTransient<IKitchenLightEntities, KitchenLightEntityAdapter>()
            .AddTransient<ICookingEntities, KitchenCookingEntityAdapter>();
    }
}
