namespace HomeAutomation.apps.Common.Settings;

[AttributeUsage(AttributeTargets.Property)]
public sealed class SettingsSelectOptionsAttribute(Type providerType) : Attribute
{
    public Type ProviderType { get; } = providerType;

    public IReadOnlyList<SettingsSelectOption> CreateOptions()
    {
        if (!typeof(ISettingsSelectOptionsProvider).IsAssignableFrom(ProviderType))
        {
            throw new InvalidOperationException(
                $"Provider type '{ProviderType.Name}' must implement {nameof(ISettingsSelectOptionsProvider)}."
            );
        }

        if (Activator.CreateInstance(ProviderType) is not ISettingsSelectOptionsProvider provider)
        {
            throw new InvalidOperationException(
                $"Provider type '{ProviderType.Name}' must have a public parameterless constructor."
            );
        }

        return provider.GetOptions();
    }
}

public interface ISettingsSelectOptionsProvider
{
    IReadOnlyList<SettingsSelectOption> GetOptions();
}

public sealed record SettingsSelectOption(string Value, string Label);
