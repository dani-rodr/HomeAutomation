using System.Linq;

namespace HomeAutomation.apps.Common.Config;

public sealed class AreaConfigRegistry(IEnumerable<AreaConfigDescriptor> descriptors)
    : IAreaConfigRegistry
{
    private readonly Dictionary<string, AreaConfigDescriptor> _descriptors =
        descriptors.ToDictionary(static item => item.Key, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<AreaConfigDescriptor> List() => _descriptors.Values.ToList();

    public bool TryGet(string areaKey, out AreaConfigDescriptor descriptor) =>
        _descriptors.TryGetValue(areaKey, out descriptor!);
}
