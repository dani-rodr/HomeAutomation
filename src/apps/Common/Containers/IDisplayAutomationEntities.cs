namespace HomeAutomation.apps.Common.Containers;

public interface IDisplayAutomationEntities
{
    SwitchEntity LgScreen { get; }
    InputNumberEntity LgTvBrightness { get; }
}

public class DeskDisplayEntities(Entities entities) : IDisplayAutomationEntities
{
    public SwitchEntity LgScreen => entities.Switch.LgScreen;
    public InputNumberEntity LgTvBrightness => entities.InputNumber.LgTvBrightness;
}
