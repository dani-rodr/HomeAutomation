using HomeAutomation.apps.Area.Bedroom.Automations.Entities;
using HomeAutomation.apps.Area.Bedroom.Devices;
using HomeAutomation.apps.Common.Config;
using Microsoft.Extensions.DependencyInjection;
using BedroomClimateEntityAdapter = HomeAutomation.apps.Area.Bedroom.Automations.Entities.ClimateEntities;
using BedroomClimateSchedulerEntities = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.Entities.GlobalClimateSchedulerEntities;
using BedroomClimateSettingsResolver = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateSettingsResolver;
using BedroomFanEntityAdapter = HomeAutomation.apps.Area.Bedroom.Automations.Entities.FanEntities;
using BedroomIAcTemperatureCalculator = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.IAcTemperatureCalculator;
using BedroomIClimateSchedulerEntities = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.Entities.IClimateSchedulerEntities;
using BedroomIClimateSettingsResolver = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.IClimateSettingsResolver;
using BedroomLightEntityAdapter = HomeAutomation.apps.Area.Bedroom.Automations.Entities.LightEntities;
using BedroomTemperatureCalculator = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.AcTemperatureCalculator;

namespace HomeAutomation.apps.Area.Bedroom;

public static class BedroomServiceCollectionExtensions
{
    public static IServiceCollection AddBedroomServices(this IServiceCollection services)
    {
        return services
            .AddAreaConfig("bedroom", "Bedroom", "Bedroom climate automation settings")
            .AddTransient<BedroomDevices>()
            .AddTransient<IBedroomLightEntities, BedroomLightEntityAdapter>()
            .AddTransient<IBedroomFanEntities, BedroomFanEntityAdapter>()
            .AddTransient<IClimateEntities, BedroomClimateEntityAdapter>()
            .AddTransient<BedroomIClimateSchedulerEntities, BedroomClimateSchedulerEntities>()
            .AddTransient<BedroomIAcTemperatureCalculator, BedroomTemperatureCalculator>()
            .AddTransient<BedroomIClimateSettingsResolver, BedroomClimateSettingsResolver>();
    }
}
