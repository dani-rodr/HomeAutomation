using HomeAutomation.apps.Area.Kitchen.Automations.Entities;
using HomeAutomation.apps.Area.Kitchen.Devices;
using HomeAutomation.apps.Common.Config;
using Microsoft.Extensions.DependencyInjection;
using KitchenCookingEntityAdapter = HomeAutomation.apps.Area.Kitchen.Automations.Entities.CookingEntities;
using KitchenLightEntityAdapter = HomeAutomation.apps.Area.Kitchen.Automations.Entities.LightEntities;

namespace HomeAutomation.apps.Area.Kitchen;

public static class KitchenServiceCollectionExtensions
{
    public static IServiceCollection AddKitchenServices(this IServiceCollection services)
    {
        return services
            .AddAreaConfig("kitchen", "Kitchen", "Kitchen automation settings")
            .AddTransient<KitchenDevices>()
            .AddTransient<IKitchenLightEntities, KitchenLightEntityAdapter>()
            .AddTransient<ICookingEntities, KitchenCookingEntityAdapter>();
    }
}
