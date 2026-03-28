using HomeAutomation.apps.Security.Automations.Entities;
using HomeAutomation.apps.Security.People;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Security;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        return services
            .AddTransient<LockDevices>()
            .AddTransient<SecurityDevices>()
            .AddTransient<ILockingEntities, LockingEntities>()
            .AddTransient<IAccessControlAutomationEntities, AccessControlAutomationEntities>()
            .AddTransient<DanielEntities>()
            .AddTransient<AthenaEntities>();
    }
}
