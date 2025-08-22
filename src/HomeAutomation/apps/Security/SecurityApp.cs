using HomeAutomation.apps.Common.Security.Automations;
using HomeAutomation.apps.Security.Automations;

namespace HomeAutomation.apps.Common.Security;

public class SecurityApp(
    ILockingEntities lockEntities,
    IAccessControlAutomationEntities locationEntities,
    INotificationServices notificationServices,
    IEventHandler eventHandler,
    DanielEntities danielEntities,
    AthenaEntities athenaEntities,
    IPersonControllerFactory personControllerFactory,
    ILoggerFactory loggerFactory
) : AppBase<SecurityApp>
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        var danielController = personControllerFactory.Create(danielEntities);
        var athenaController = personControllerFactory.Create(athenaEntities);

        yield return danielController;
        yield return athenaController;
        yield return new AccessControlAutomation(
            [danielController, athenaController],
            locationEntities,
            loggerFactory.CreateLogger<AccessControlAutomation>()
        );
        yield return new LockAutomation(
            lockEntities,
            notificationServices,
            eventHandler,
            loggerFactory.CreateLogger<LockAutomation>()
        );
    }
}
