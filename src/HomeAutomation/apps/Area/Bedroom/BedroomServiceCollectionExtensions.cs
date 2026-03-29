using HomeAutomation.apps.Area.Bedroom.Automations.Entities;
using HomeAutomation.apps.Area.Bedroom.Devices;
using Microsoft.Extensions.DependencyInjection;
using BedroomClimateEntityAdapter = HomeAutomation.apps.Area.Bedroom.Automations.Entities.ClimateEntities;
using BedroomFanEntityAdapter = HomeAutomation.apps.Area.Bedroom.Automations.Entities.FanEntities;
using BedroomLightEntityAdapter = HomeAutomation.apps.Area.Bedroom.Automations.Entities.LightEntities;

namespace HomeAutomation.apps.Area.Bedroom;

public static class BedroomServiceCollectionExtensions
{
    public static IServiceCollection AddBedroomServices(this IServiceCollection services)
    {
        return services
            .AddTransient<BedroomDevices>()
            .AddTransient<IBedroomLightEntities, BedroomLightEntityAdapter>()
            .AddTransient<IBedroomFanEntities, BedroomFanEntityAdapter>()
            .AddTransient<IClimateEntities, BedroomClimateEntityAdapter>()
            .AddTransient<IClimateSchedulerEntities, GlobalClimateSchedulerEntities>()
            .AddTransient<IAcTemperatureCalculator, AcTemperatureCalculator>()
            .AddTransient<IClimateScheduler, ClimateScheduler>();
    }
}
