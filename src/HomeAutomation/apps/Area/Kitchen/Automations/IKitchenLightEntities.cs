namespace HomeAutomation.apps.Area.Kitchen.Automations;

public interface IKitchenLightEntities : ILightAutomationEntities
{
    BinarySensorEntity PowerPlug { get; }
}
