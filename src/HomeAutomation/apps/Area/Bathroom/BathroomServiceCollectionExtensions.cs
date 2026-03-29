using HomeAutomation.apps.Area.Bathroom.Automations.Entities;
using HomeAutomation.apps.Area.Bathroom.Devices;
using Microsoft.Extensions.DependencyInjection;

using BathroomLightEntityAdapter = HomeAutomation.apps.Area.Bathroom.Automations.Entities.LightEntities;

namespace HomeAutomation.apps.Area.Bathroom;

public static class BathroomServiceCollectionExtensions
{
    public static IServiceCollection AddBathroomServices(this IServiceCollection services)
    {
        return services
            .AddTransient<BathroomDevices>()
            .AddTransient<IBathroomLightEntities, BathroomLightEntityAdapter>();
    }
}
