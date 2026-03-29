namespace HomeAutomation.apps.Area.Bedroom.Automations.Entities;

public interface IBedroomLightEntities : ILightAutomationEntities
{
    SwitchEntity RightSideEmptySwitch { get; }
    SwitchEntity LeftSideFanSwitch { get; }
}
