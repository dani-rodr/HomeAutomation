using HomeAutomation.apps.Area.Bathroom.Automations;
using HomeAutomation.apps.Area.Bathroom.Devices;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Area.Bathroom;

public static class BathroomServiceCollectionExtensions
{
    public static IServiceCollection AddBathroomServices(this IServiceCollection services)
    {
        return services
            .AddTransient<BathroomDevices>()
            .AddTransient<IBathroomLightEntities, BathroomLightEntities>();
    }
}
