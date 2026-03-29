namespace HomeAutomation.apps.Area.Kitchen.Automations.Entities;

public interface IKitchenLightEntities : ILightAutomationEntities
{
    BinarySensorEntity PowerPlug { get; }
}
