using HomeAutomation.apps.Security.Automations;
using HomeAutomation.apps.Security.Automations.Entities;
using HomeAutomation.apps.Security.People;

namespace HomeAutomation.apps.Security;

[AreaKey("security")]
public class SecurityApp(
    ILockingEntities lockEntities,
    IAccessControlAutomationEntities locationEntities,
    INotificationServices notificationServices,
    IEventHandler eventHandler,
    DanielEntities danielEntities,
    AthenaEntities athenaEntities,
    IPersonControllerFactory personControllerFactory,
    ILogger<AccessControlAutomation> accessControlAutomationLogger,
    ILogger<LockAutomation> lockAutomationLogger
) : AppBase<SecurityApp, NoAppSettings>(NoAppSettings.Instance)
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
            accessControlAutomationLogger
        );
        yield return new LockAutomation(
            lockEntities,
            notificationServices,
            eventHandler,
            lockAutomationLogger
        );
    }
}
