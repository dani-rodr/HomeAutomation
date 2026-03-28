using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Area.LivingRoom.Devices;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Area.LivingRoom;

public static class LivingRoomServiceCollectionExtensions
{
    public static IServiceCollection AddLivingRoomServices(this IServiceCollection services)
    {
        return services
            .AddTransient<LivingRoomLightDevices>()
            .AddTransient<LivingRoomFanDevices>()
            .AddTransient<LivingRoomAirQualityDevices>()
            .AddTransient<LivingRoomMediaDevices>()
            .AddTransient<ILivingRoomLightEntities, LivingRoomLightEntities>()
            .AddTransient<ILivingRoomFanEntities, LivingRoomFanEntities>()
            .AddTransient<IAirQualityEntities, AirQualityEntities>()
            .AddTransient<ITabletEntities, LivingRoomTabletEntities>()
            .AddTransient<ITclDisplayEntities, TclDisplayEntities>()
            .AddTransient<ITclDisplay, TclDisplay>();
    }
}
