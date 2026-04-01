using HomeAutomation.apps.Area.Bathroom;
using HomeAutomation.apps.Area.Bedroom;
using HomeAutomation.apps.Area.Desk;
using HomeAutomation.apps.Area.Kitchen;
using HomeAutomation.apps.Area.LivingRoom;
using HomeAutomation.apps.Area.Pantry;
using HomeAutomation.apps.Common.Devices;
using HomeAutomation.apps.Common.Services.Logging;
using HomeAutomation.apps.Common.Settings;
using HomeAutomation.apps.Security;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHomeEntitiesAndServices(this IServiceCollection services)
    {
        return services
            .AddAreaSettingsEngine()
            .AddTransient<IEventHandler, HaEventHandler>()
            .AddTransient<GlobalDevices>()
            .AddTransient<ITypedEntityFactory, EntityFactory>()
            .AddTransient<IServices, Services>()
            .AddTransient<INotificationServices, NotificationServices>()
            .AddSingleton<IAutomationLogPolicy, AutomationLogPolicy>()
            .AddSingleton<ILogbookSink, LogbookSink>()
            .AddTransient(typeof(ILogger<>), typeof(AutomationLogger<>))
            .AddTransient<IWebhookServices>(p => new WebhookServices(
                p.GetRequiredService<ITriggerManager>(),
                p.GetRequiredService<ILogger<WebhookServices>>()
            ))
            .AddTransient<IDimmingLightControllerFactory, DimmingLightControllerFactory>()
            .AddTransient<IPersonControllerFactory, PersonControllerFactory>()
            .AddTransient<IMotionSensorRestartScheduler, MotionSensorRestartScheduler>()
            .AddSecurityServices()
            .AddBedroomServices()
            .AddDeskServices()
            .AddBathroomServices()
            .AddKitchenServices()
            .AddLivingRoomServices()
            .AddPantryServices()
            .AddMotionSensors();
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
}
