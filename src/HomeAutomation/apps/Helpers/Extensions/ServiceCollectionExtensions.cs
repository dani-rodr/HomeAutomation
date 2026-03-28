using HomeAutomation.apps.Area.Bathroom;
using HomeAutomation.apps.Area.Bedroom;
using HomeAutomation.apps.Area.Desk;
using HomeAutomation.apps.Area.Kitchen;
using HomeAutomation.apps.Area.LivingRoom.Devices;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHomeEntitiesAndServices(this IServiceCollection services)
    {
        return services
            .AddTransient<IEventHandler, HaEventHandler>()
            .AddTransient<Devices>()
            .AddTransient<ILockingEntities, LockingEntities>()
            .AddTransient<ITypedEntityFactory, EntityFactory>()
            .AddTransient<IServices, Services>()
            .AddTransient<INotificationServices, NotificationServices>()
            .AddTransient<IWebhookServices>(p => new WebhookServices(
                p.GetRequiredService<ITriggerManager>(),
                p.GetRequiredService<ILogger<WebhookServices>>()
            ))
            .AddTransient<IDimmingLightControllerFactory, DimmingLightControllerFactory>()
            .AddTransient<IPersonControllerFactory, PersonControllerFactory>()
            .AddTransient<IMotionSensorRestartScheduler, MotionSensorRestartScheduler>()
            .AddTransient<IAccessControlAutomationEntities, AccessControlAutomationEntities>()
            .AddTransient<DanielEntities>()
            .AddTransient<AthenaEntities>()
            .AddAreaEntities()
            .AddMotionSensors();
    }

    private static IServiceCollection AddAreaEntities(this IServiceCollection services)
    {
        return services
            .AddBedroomEntities()
            .AddDeskEntities()
            .AddBathroomEntities()
            .AddKitchenEntities()
            .AddLivingRoomEntities()
            .AddPantryEntities();
    }

    private static IServiceCollection AddMotionSensors(this IServiceCollection services)
    {
        return services
            .AddTransient<Area.Bathroom.Devices.MotionSensor>()
            .AddTransient<Area.Bedroom.Devices.MotionSensor>()
            .AddTransient<Area.LivingRoom.Devices.MotionSensor>()
            .AddTransient<Area.Kitchen.Devices.MotionSensor>()
            .AddTransient<Area.Desk.Devices.MotionSensor>()
            .AddTransient<Area.Pantry.Devices.MotionSensor>();
    }

    private static IServiceCollection AddBedroomEntities(this IServiceCollection services)
    {
        return services.AddBedroomServices();
    }

    private static IServiceCollection AddDeskEntities(this IServiceCollection services)
    {
        return services.AddDeskServices();
    }

    private static IServiceCollection AddBathroomEntities(this IServiceCollection services)
    {
        return services.AddBathroomServices();
    }

    private static IServiceCollection AddKitchenEntities(this IServiceCollection services)
    {
        return services.AddKitchenServices();
    }

    private static IServiceCollection AddLivingRoomEntities(this IServiceCollection services)
    {
        return services
            .AddTransient<ILivingRoomLightEntities, LivingRoomLightEntities>()
            .AddTransient<ILivingRoomFanEntities, LivingRoomFanEntities>()
            .AddTransient<IAirQualityEntities, AirQualityEntities>()
            .AddTransient<ITabletEntities, LivingRoomTabletEntities>()
            .AddTransient<ITclDisplayEntities, TclDisplayEntities>()
            .AddTransient<ITclDisplay, TclDisplay>();
    }

    private static IServiceCollection AddPantryEntities(this IServiceCollection services)
    {
        return services.AddTransient<IPantryLightEntities, PantryLightEntities>();
    }
}
