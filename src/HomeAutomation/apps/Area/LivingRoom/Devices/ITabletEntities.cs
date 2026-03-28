namespace HomeAutomation.apps.Area.LivingRoom.Devices;

public interface ITabletEntities : ILightAutomationEntities
{
    BinarySensorEntity TabletActive { get; }
}
