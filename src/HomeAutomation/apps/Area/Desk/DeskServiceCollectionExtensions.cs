using HomeAutomation.apps.Area.Desk.Automations.Entities;
using HomeAutomation.apps.Area.Desk.Devices;
using HomeAutomation.apps.Area.Desk.Devices.Entities;
using HomeAutomation.apps.Area.Desk.Services.Entities;
using HomeAutomation.apps.Area.Desk.Services.Schedulers.Entities;
using Microsoft.Extensions.DependencyInjection;
using DeskLightEntityAdapter = HomeAutomation.apps.Area.Desk.Automations.Entities.LightEntities;

namespace HomeAutomation.apps.Area.Desk;

public static class DeskServiceCollectionExtensions
{
    public static IServiceCollection AddDeskServices(this IServiceCollection services)
    {
        return services
            .AddTransient<DeskDevices>()
            .AddTransient<IDeskLightEntities, DeskLightEntityAdapter>()
            .AddTransient<ILgDisplayEntities, LgDisplayEntities>()
            .AddTransient<IDesktopEntities, DeskDesktopEntities>()
            .AddTransient<ILaptopEntities, LaptopEntities>()
            .AddTransient<ILaptopSchedulerEntities, LaptopSchedulerEntities>()
            .AddTransient<IChargingHandlerEntities, LaptopChargingHandlerEntities>()
            .AddTransient<ILaptopChargingHandler, LaptopChargingHandler>()
            .AddTransient<ILaptopShutdownScheduler, LaptopScheduler>()
            .AddTransient<ILgDisplay, LgDisplay>()
            .AddTransient<IDesktop, Desktop>()
            .AddTransient<ILaptop, Laptop>();
    }
}
