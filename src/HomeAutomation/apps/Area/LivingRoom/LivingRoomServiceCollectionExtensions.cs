using HomeAutomation.apps.Area.LivingRoom.Automations.Entities;
using HomeAutomation.apps.Area.LivingRoom.Devices;
using HomeAutomation.apps.Area.LivingRoom.Devices.Entities;
using HomeAutomation.apps.Common.Config;
using Microsoft.Extensions.DependencyInjection;
using LivingRoomFanEntityAdapter = HomeAutomation.apps.Area.LivingRoom.Automations.Entities.FanEntities;
using LivingRoomLightEntityAdapter = HomeAutomation.apps.Area.LivingRoom.Automations.Entities.LightEntities;
using LivingRoomTabletEntityAdapter = HomeAutomation.apps.Area.LivingRoom.Automations.Entities.TabletEntities;

namespace HomeAutomation.apps.Area.LivingRoom;

public static class LivingRoomServiceCollectionExtensions
{
    public static IServiceCollection AddLivingRoomServices(this IServiceCollection services)
    {
        return services
            .AddAreaConfig("livingroom", "Living Room", "Living room automation settings")
            .AddTransient<LivingRoomLightDevices>()
            .AddTransient<LivingRoomFanDevices>()
            .AddTransient<LivingRoomAirQualityDevices>()
            .AddTransient<LivingRoomMediaDevices>()
            .AddTransient<ILivingRoomLightEntities, LivingRoomLightEntityAdapter>()
            .AddTransient<ILivingRoomFanEntities, LivingRoomFanEntityAdapter>()
            .AddTransient<IAirQualityEntities, AirQualityEntities>()
            .AddTransient<ITabletEntities, LivingRoomTabletEntityAdapter>()
            .AddTransient<ITclDisplayEntities, TclDisplayEntities>()
            .AddTransient<ITclDisplay, TclDisplay>();
    }
}
