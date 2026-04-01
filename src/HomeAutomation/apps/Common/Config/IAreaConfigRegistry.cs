namespace HomeAutomation.apps.Common.Config;

public interface IAreaConfigRegistry
{
    IReadOnlyCollection<AreaConfigDescriptor> List();

    bool TryGet(string areaKey, out AreaConfigDescriptor descriptor);
}
