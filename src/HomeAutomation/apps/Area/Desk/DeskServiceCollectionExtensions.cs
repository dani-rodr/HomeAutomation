using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Area.Desk.Devices;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Area.Desk;

public static class DeskServiceCollectionExtensions
{
    public static IServiceCollection AddDeskServices(this IServiceCollection services)
    {
        return services
            .AddTransient<DeskDevices>()
            .AddTransient<IDeskLightEntities, DeskLightEntities>()
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
