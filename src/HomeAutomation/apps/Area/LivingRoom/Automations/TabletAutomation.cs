namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class TabletAutomation(ITabletEntities entities, ILogger<TabletAutomation> logger)
    : LightAutomationBase(entities, logger)
{
    protected override IEnumerable<IDisposable> GetSensorDelayAutomations() => [];

    protected override IEnumerable<IDisposable> GetLightAutomations() => [];
}
