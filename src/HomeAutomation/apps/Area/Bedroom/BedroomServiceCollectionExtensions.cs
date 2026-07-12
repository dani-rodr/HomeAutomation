using HomeAutomation.apps.Area.Bedroom.Automations.Entities;
using HomeAutomation.apps.Area.Bedroom.Devices;
using Microsoft.Extensions.DependencyInjection;
using BedroomClimateAutomationScheduler = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateAutomationScheduler;
using BedroomClimateEntityAdapter = HomeAutomation.apps.Area.Bedroom.Automations.Entities.ClimateEntities;
using BedroomFanEntityAdapter = HomeAutomation.apps.Area.Bedroom.Automations.Entities.FanEntities;
using BedroomIAcTemperatureCalculator = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.IAcTemperatureCalculator;
using BedroomIClimateAutomationScheduler = HomeAutomation.apps.Area.Bedroom.Services.Schedulers.IClimateAutomationScheduler;
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
            .AddTransient<BedroomIAcTemperatureCalculator, BedroomTemperatureCalculator>()
            .AddTransient<BedroomIClimateAutomationScheduler, BedroomClimateAutomationScheduler>();
    }
}
