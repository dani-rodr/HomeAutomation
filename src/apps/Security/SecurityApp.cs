using HomeAutomation.apps.Common.Security.Automations;

namespace HomeAutomation.apps.Common.Security;

public class SecurityApp(
    ILockingEntities lockEntities,
    INotificationServices notificationServices,
    IServices services,
    IEventHandler eventHandler,
    DanielEntities danielEntities,
    AthenaEntities athenaEntities,
    ILoggerFactory loggerFactory
) : AppBase<SecurityApp>
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new PersonController(danielEntities, services);
        yield return new PersonController(athenaEntities, services);
        yield return new LockAutomation(
            lockEntities,
            notificationServices,
            eventHandler,
            loggerFactory.CreateLogger<LockAutomation>()
        );
    }
}
