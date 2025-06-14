using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IEventHandler, HaEventHandler>()
            .AddTransientEntity<CommonEntities, CommonEntities>()
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
            .AddTransientEntity<IFanAutomationEntities, BedroomFanEntities>()
            .AddTransientEntity<IClimateAutomationEntities, BedroomClimateEntities>();
    }

    private static IServiceCollection AddDeskEntities(this IServiceCollection services)
    {
        return services
            .AddTransientEntity<IDisplayAutomationEntities, DeskDisplayEntities>()
            .AddTransientEntity<ILgDisplayEntities, DeskLgDisplayEntities>()
            .AddTransientEntity<IDesktopEntities, DeskDesktopEntities>()
            .AddTransientEntity<ILaptopEntities, LaptopEntities>();
    }

    private static IServiceCollection AddBathroomEntities(this IServiceCollection services)
    {
        return services.AddTransientEntity<IBathroomMotionEntities, BathroomMotionEntities>();
    }

    private static IServiceCollection AddKitchenEntities(this IServiceCollection services)
    {
        return services
            .AddTransientEntity<IKitchenMotionEntities, KitchenMotionEntities>()
            .AddTransientEntity<ICookingAutomationEntities, KitchenCookingEntities>();
    }

    private static IServiceCollection AddLivingRoomEntities(this IServiceCollection services)
    {
        return services
            .AddTransientEntity<ILivingRoomMotionEntities, LivingRoomMotionEntities>()
            .AddTransientEntity<ILivingRoomFanEntities, LivingRoomFanEntities>()
            .AddTransientEntity<IAirQualityEntities, AirQualityEntities>()
            .AddTransientEntity<ITabletAutomationEntities, LivingRoomTabletEntities>();
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
