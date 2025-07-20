using System.Text;

namespace HomeAutomation.apps.Common.Services.Factories;

public interface ITypedEntityFactory
{
    public T Create<T>(string deviceName, string entityId)
        where T : Entity;
    public T Create<T>(string entityId)
        where T : Entity;
}

public class EntityFactory(IHaContext haContext, ILogger<EntityFactory> logger)
    : ITypedEntityFactory
{
    private static readonly Dictionary<Type, string> DomainOverrides = new()
    {
        [typeof(NumericSensorEntity)] = "sensor",
    };

    public T Create<T>(string entityId)
        where T : Entity => Create<T>(string.Empty, entityId);

    public T Create<T>(string deviceName, string entityId)
        where T : Entity
    {
        var domain = GetDomainFromType<T>();
        var prefix = string.IsNullOrEmpty(deviceName) ? "" : deviceName + "_";
        var fullEntityId = $"{domain}.{prefix}{entityId}".ToLower();

        var ctor =
            typeof(T).GetConstructor([typeof(IHaContext), typeof(string)])
            ?? throw new InvalidOperationException(
                $"No suitable constructor found for {typeof(T).Name}"
            );
        var entity = (T)ctor.Invoke([haContext, fullEntityId]);
        logger.LogDebug("Created entity {EntityId}", entity.EntityId);
        return entity;
    }

    private static string GetDomainFromType<T>()
        where T : Entity
    {
        if (DomainOverrides.TryGetValue(typeof(T), out var domainOverride))
        {
            return domainOverride;
        }

        var typeName = typeof(T).Name;
        if (!typeName.EndsWith("Entity"))
        {
            throw new InvalidOperationException($"Unexpected entity type name: {typeName}");
        }

        return ToSnakeCase(typeName[..^"Entity".Length]);
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var sb = new StringBuilder();
        sb.Append(char.ToLowerInvariant(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(input[i]));
            }
            else
            {
                sb.Append(input[i]);
            }
        }

        return sb.ToString();
    }
}
