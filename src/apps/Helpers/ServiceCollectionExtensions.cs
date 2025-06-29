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
            .AddTransient<CommonEntities>()
            .AddTransientEntity<ILockingEntities, LockingEntities>()
            .AddScoped<INotificationServices>(p => new NotificationServices(
                p.GetRequiredService<Services>(),
                p.GetRequiredService<ILogger<NotificationServices>>()
            ))
            .AddScoped<IWebhookServices>(p => new WebhookServices(
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
            .AddTransientEntity<IBedroomMotionEntities, BedroomMotionEntities>()
            .AddTransientEntity<IBedroomFanEntities, BedroomFanEntities>()
            .AddTransientEntity<IClimateEntities, BedroomClimateEntities>()
            .AddTransientEntity<IClimateSchedulerEntities, ClimateSchedulerEntities>()
            .AddTransient<IClimateScheduler>(p => new ClimateScheduler(
                p.GetRequiredService<IClimateSchedulerEntities>(),
                p.GetRequiredService<IScheduler>(),
                p.GetRequiredService<ILogger<ClimateScheduler>>()
            ));
    }

    private static IServiceCollection AddDeskEntities(this IServiceCollection services)
    {
        return services
            .AddTransientEntity<IDeskMotionEntities, DeskMotionEntities>()
            .AddTransientEntity<ILgDisplayEntities, LgDisplayEntities>()
            .AddTransientEntity<IDesktopEntities, DeskDesktopEntities>()
            .AddTransientEntity<ILaptopEntities, LaptopEntities>()
            .AddTransientEntity<ILaptopSchedulerEntities, LaptopSchedulerEntities>()
            .AddTransientEntity<IChargingHandlerEntities, LaptopChargingHandlerEntities>()
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
        return services.AddTransientEntity<IBathroomMotionEntities, BathroomMotionEntities>();
    }

    private static IServiceCollection AddKitchenEntities(this IServiceCollection services)
    {
        return services
            .AddTransientEntity<IKitchenMotionEntities, KitchenMotionEntities>()
            .AddTransientEntity<ICookingEntities, KitchenCookingEntities>();
    }

    private static IServiceCollection AddLivingRoomEntities(this IServiceCollection services)
    {
        return services
            .AddTransientEntity<ILivingRoomMotionEntities, LivingRoomMotionEntities>()
            .AddTransientEntity<ILivingRoomFanEntities, LivingRoomFanEntities>()
            .AddTransientEntity<IAirQualityEntities, AirQualityEntities>()
            .AddTransientEntity<ITabletEntities, LivingRoomTabletEntities>()
            .AddTransientEntity<ITclDisplayEntities, TclDisplayEntities>()
            .AddTransient<ITclDisplay>(p => new TclDisplay(
                p.GetRequiredService<ITclDisplayEntities>(),
                p.GetRequiredService<ILogger<TclDisplay>>()
            ));
    }

    private static IServiceCollection AddPantryEntities(this IServiceCollection services)
    {
        return services.AddTransientEntity<IPantryMotionEntities, PantryMotionEntities>();
    }

    private static IServiceCollection AddTransientEntity<TInterface, TImplementation>(
        this IServiceCollection services
    )
        where TInterface : class
        where TImplementation : class, TInterface
    {
        return services.AddTransient<TInterface>(sp =>
            ActivatorUtilities.CreateInstance<TImplementation>(sp)
        );
    }
}
