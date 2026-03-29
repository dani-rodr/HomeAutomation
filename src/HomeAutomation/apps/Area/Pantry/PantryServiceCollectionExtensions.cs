using HomeAutomation.apps.Area.Pantry.Automations.Entities;
using HomeAutomation.apps.Area.Pantry.Devices;
using Microsoft.Extensions.DependencyInjection;
using PantryLightEntityAdapter = HomeAutomation.apps.Area.Pantry.Automations.Entities.LightEntities;

namespace HomeAutomation.apps.Area.Pantry;

public static class PantryServiceCollectionExtensions
{
    public static IServiceCollection AddPantryServices(this IServiceCollection services)
    {
        return services
            .AddTransient<PantryDevices>()
            .AddTransient<IPantryLightEntities, PantryLightEntityAdapter>();
    }
}
