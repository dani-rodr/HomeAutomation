using HomeAutomation.apps.Area.Bedroom.Automations.Entities;
using HomeAutomation.apps.Area.Bedroom.Devices;
using Microsoft.Extensions.DependencyInjection;
using BedroomClimateEntityAdapter = HomeAutomation.apps.Area.Bedroom.Automations.Entities.ClimateEntities;
using BedroomClimateScheduler = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateScheduler;
using BedroomClimateSchedulerEntities = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.Entities.GlobalClimateSchedulerEntities;
using BedroomFanEntityAdapter = HomeAutomation.apps.Area.Bedroom.Automations.Entities.FanEntities;
using BedroomIAcTemperatureCalculator = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.IAcTemperatureCalculator;
using BedroomIClimateScheduler = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.IClimateScheduler;
using BedroomIClimateSchedulerEntities = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.Entities.IClimateSchedulerEntities;
using BedroomLightEntityAdapter = HomeAutomation.apps.Area.Bedroom.Automations.Entities.LightEntities;
using BedroomTemperatureCalculator = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.AcTemperatureCalculator;

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
            .AddTransient<BedroomIClimateSchedulerEntities, BedroomClimateSchedulerEntities>()
            .AddTransient<BedroomIAcTemperatureCalculator, BedroomTemperatureCalculator>()
            .AddTransient<BedroomIClimateScheduler, BedroomClimateScheduler>();
    }
}
