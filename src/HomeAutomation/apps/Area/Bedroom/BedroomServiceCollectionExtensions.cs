using HomeAutomation.apps.Area.Bedroom.Automations;
using HomeAutomation.apps.Area.Bedroom.Devices;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Area.Bedroom;

public static class BedroomServiceCollectionExtensions
{
    public static IServiceCollection AddBedroomServices(this IServiceCollection services)
    {
        return services
            .AddTransient<BedroomDevices>()
            .AddTransient<IBedroomLightEntities, BedroomLightEntities>()
            .AddTransient<IBedroomFanEntities, BedroomFanEntities>()
            .AddTransient<IClimateEntities, BedroomClimateEntities>()
            .AddTransient<IClimateSchedulerEntities, GlobalClimateSchedulerEntities>()
            .AddTransient<IAcTemperatureCalculator, AcTemperatureCalculator>()
            .AddTransient<IClimateScheduler, ClimateScheduler>();
    }
}
