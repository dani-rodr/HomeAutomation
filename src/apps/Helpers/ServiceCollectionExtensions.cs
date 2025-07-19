using HomeAutomation.apps.Area.Desk.Devices;
using HomeAutomation.apps.Area.LivingRoom.Devices;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IEventHandler, HaEventHandler>()
            .AddTransient<Devices>()
            .AddTransient<ILockingEntities, LockingEntities>()
            .AddTransient<ITypedEntityFactory, EntityFactory>()
            .AddTransient<INotificationServices>(p => new NotificationServices(
                p.GetRequiredService<Services>(),
                p.GetRequiredService<ILogger<NotificationServices>>()
            ))
            .AddTransient<IWebhookServices>(p => new WebhookServices(
                p.GetRequiredService<ITriggerManager>(),
                p.GetRequiredService<ILogger<WebhookServices>>()
            ));
    }

    public static IServiceCollection AddAreaEntities(this IServiceCollection services)
    {
        return services
            .AddBedroomEntities()
            .AddDeskEntities()
            .AddBathroomEntities()
            .AddKitchenEntities()
            .AddLivingRoomEntities()
            .AddPantryEntities();
    }

    private static IServiceCollection AddBedroomEntities(this IServiceCollection services)
    {
        return services
            .AddTransient<IBedroomLightEntities, BedroomLightEntities>()
            .AddTransient<IBedroomFanEntities, BedroomFanEntities>()
            .AddTransient<IClimateEntities, BedroomClimateEntities>()
            .AddTransient<IClimateSchedulerEntities, ClimateSchedulerEntities>()
            .AddTransient<IAcTemperatureCalculator>(p => new AcTemperatureCalculator(
                p.GetRequiredService<IClimateSchedulerEntities>(),
                p.GetRequiredService<ILogger<AcTemperatureCalculator>>()
            ))
            .AddTransient<IClimateScheduler>(p => new ClimateScheduler(
                p.GetRequiredService<IClimateSchedulerEntities>(),
                p.GetRequiredService<IScheduler>(),
                p.GetRequiredService<IAcTemperatureCalculator>(),
                p.GetRequiredService<ILogger<ClimateScheduler>>()
            ));
    }

    private static IServiceCollection AddDeskEntities(this IServiceCollection services)
    {
        return services
            .AddTransient<IDeskLightEntities, DeskLightEntities>()
            .AddTransient<ILgDisplayEntities, LgDisplayEntities>()
            .AddTransient<IDesktopEntities, DeskDesktopEntities>()
            .AddTransient<ILaptopEntities, LaptopEntities>()
            .AddTransient<ILaptopSchedulerEntities, LaptopSchedulerEntities>()
            .AddTransient<IChargingHandlerEntities, LaptopChargingHandlerEntities>()
            .AddTransient<IChargingHandler>(p => new ChargingHandler(
                p.GetRequiredService<IChargingHandlerEntities>()
            ))
            .AddTransient<ILaptopChargingHandler>(p => new LaptopChargingHandler(
                p.GetRequiredService<IChargingHandlerEntities>(),
                p.GetRequiredService<IScheduler>()
            ))
            .AddTransient<ILaptopScheduler>(p => new LaptopScheduler(
                p.GetRequiredService<ILaptopSchedulerEntities>(),
                p.GetRequiredService<IScheduler>()
            ))
            .AddTransient<ILgDisplay>(p => new LgDisplay(
                p.GetRequiredService<ILgDisplayEntities>(),
                p.GetRequiredService<Services>(),
                p.GetRequiredService<ILogger<LgDisplay>>()
            ));
    }

    private static IServiceCollection AddBathroomEntities(this IServiceCollection services)
    {
        return services.AddTransient<IBathroomLightEntities, BathroomLightEntities>();
    }

    private static IServiceCollection AddKitchenEntities(this IServiceCollection services)
    {
        return services
            .AddTransient<IKitchenLightEntities, KitchenLightEntities>()
            .AddTransient<ICookingEntities, KitchenCookingEntities>();
    }

    private static IServiceCollection AddLivingRoomEntities(this IServiceCollection services)
    {
        return services
            .AddTransient<ILivingRoomLightEntities, LivingRoomLightEntities>()
            .AddTransient<ILivingRoomFanEntities, LivingRoomFanEntities>()
            .AddTransient<IAirQualityEntities, AirQualityEntities>()
            .AddTransient<ITabletEntities, LivingRoomTabletEntities>()
            .AddTransient<ITclDisplayEntities, TclDisplayEntities>()
            .AddTransient<ITclDisplay>(p => new TclDisplay(
                p.GetRequiredService<ITclDisplayEntities>(),
                p.GetRequiredService<ILogger<TclDisplay>>()
            ));
    }

    private static IServiceCollection AddPantryEntities(this IServiceCollection services)
    {
        return services.AddTransient<IPantryLightEntities, PantryLightEntities>();
    }
}
