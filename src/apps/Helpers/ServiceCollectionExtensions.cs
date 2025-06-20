using HomeAutomation.apps.Area.Desk.Devices;
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
            .AddTransient<INotificationServices>(p => new NotificationServices(
                p.GetRequiredService<Services>(),
                p.GetRequiredService<ILogger<NotificationServices>>()
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
            .AddTransientEntity<IClimateEntities, BedroomClimateEntities>();
    }

    private static IServiceCollection AddDeskEntities(this IServiceCollection services)
    {
        return services
            .AddTransientEntity<IDeskMotionEntities, DeskMotionEntities>()
            .AddTransientEntity<ILgDisplayEntities, DeskLgDisplayEntities>()
            .AddTransientEntity<IDesktopEntities, DeskDesktopEntities>()
            .AddTransientEntity<ILaptopEntities, LaptopEntities>()
            .AddTransientEntity<ILaptopSchedulerEntities, LaptopSchedulerEntities>()
            .AddTransientEntity<IBatteryHandlerEntities, LaptopBatteryHandlerEntities>()
            .AddTransientEntity<IBatteryHandler, LaptopBatteryHandler>()
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
            .AddTransientEntity<ITabletEntities, LivingRoomTabletEntities>();
    }

    private static IServiceCollection AddPantryEntities(this IServiceCollection services)
    {
        return services.AddTransientEntity<IPantryMotionEntities, PantryMotionEntities>();
    }

    private static IServiceCollection AddTransientEntity<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        return services.AddTransient<TInterface>(sp => ActivatorUtilities.CreateInstance<TImplementation>(sp));
    }
}
