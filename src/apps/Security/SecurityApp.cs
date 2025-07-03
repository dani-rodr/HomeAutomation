using HomeAutomation.apps.Common.Security.Automations;

namespace HomeAutomation.apps.Common.Security;

public class SecurityApp(
    ILockingEntities lockEntities,
    INotificationServices notificationServices,
    IEventHandler eventHandler,
    ILoggerFactory loggerFactory
) : AppBase<SecurityApp>
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new LockAutomation(
            lockEntities,
            notificationServices,
            eventHandler,
            loggerFactory.CreateLogger<LockAutomation>()
        );
    }
}
