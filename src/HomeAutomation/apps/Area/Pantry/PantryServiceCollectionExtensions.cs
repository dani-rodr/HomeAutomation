using HomeAutomation.apps.Area.Pantry.Automations;
using HomeAutomation.apps.Area.Pantry.Devices;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Area.Pantry;

public static class PantryServiceCollectionExtensions
{
    public static IServiceCollection AddPantryServices(this IServiceCollection services)
    {
        return services
            .AddTransient<PantryDevices>()
            .AddTransient<IPantryLightEntities, PantryLightEntities>();
    }
}
