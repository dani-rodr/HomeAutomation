namespace HomeAutomation.apps.Area.Bedroom.Automations;

public interface IBedroomLightEntities : ILightAutomationEntities
{
    SwitchEntity RightSideEmptySwitch { get; }
    SwitchEntity LeftSideFanSwitch { get; }
}
