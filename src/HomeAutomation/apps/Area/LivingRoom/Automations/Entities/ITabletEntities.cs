namespace HomeAutomation.apps.Area.LivingRoom.Automations.Entities;

public interface ITabletEntities : ILightAutomationEntities
{
    BinarySensorEntity TabletActive { get; }
}
