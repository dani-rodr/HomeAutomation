using System.Text;

namespace HomeAutomation.apps.Common.Services;

public interface ITypedEntityFactory
{
    public T Create<T>(string entityId)
        where T : Entity;
    public string DeviceName { get; set; }
}

public class EntityFactory(IHaContext haContext) : ITypedEntityFactory
{
    public string DeviceName { get; set; } = string.Empty;

    public T Create<T>(string entityId)
        where T : Entity
    {
        var domain = GetDomainFromType<T>();
        var prefix = string.IsNullOrEmpty(DeviceName) ? "" : DeviceName + "_";
        var fullEntityId = $"{domain}.{prefix}{entityId}".ToLower();

        var ctor =
            typeof(T).GetConstructor([typeof(IHaContext), typeof(string)])
            ?? throw new InvalidOperationException(
                $"No suitable constructor found for {typeof(T).Name}"
            );
        return (T)ctor.Invoke([haContext, fullEntityId]);
    }

    private static string GetDomainFromType<T>()
        where T : Entity
    {
        var typeName = typeof(T).Name;

        if (!typeName.EndsWith("Entity"))
        {
            throw new InvalidOperationException($"Unexpected entity type name: {typeName}");
        }

        var domainPascal = typeName[..^"Entity".Length];
        var domain = ToSnakeCase(domainPascal);
        return domain;
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
