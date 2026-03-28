using HomeAutomation.apps.Area.Bedroom.Automations;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Area.Bedroom;

public static class BedroomServiceCollectionExtensions
{
    public static IServiceCollection AddBedroomServices(this IServiceCollection services)
    {
        return services
            .AddTransient<IBedroomLightEntities, BedroomLightEntities>()
            .AddTransient<IBedroomFanEntities, BedroomFanEntities>()
            .AddTransient<IClimateEntities, BedroomClimateEntities>()
            .AddTransient<IClimateSchedulerEntities, ClimateSchedulerEntities>()
            .AddTransient<IAcTemperatureCalculator, AcTemperatureCalculator>()
            .AddTransient<IClimateScheduler, ClimateScheduler>();
    }
}
