using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HomeAutomation.apps.Common.Settings;

public static class AreaSettingsValidator
{
    public static IReadOnlyDictionary<string, string[]> Validate(object settings)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        ValidateObject(settings, string.Empty, errors);

        return errors.ToDictionary(x => x.Key, x => x.Value.Distinct().ToArray());
    }

    private static void ValidateObject(
        object instance,
        string path,
        IDictionary<string, List<string>> errors
    )
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(instance);
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);

        foreach (var result in results)
        {
            var targetNames = result.MemberNames.Any() ? result.MemberNames : [string.Empty];

            foreach (var memberName in targetNames)
            {
                var key = BuildPath(path, memberName);
                AddError(errors, key, result.ErrorMessage ?? "Validation failed.");
            }
        }

        var properties = instance
            .GetType()
            .GetProperties(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
            )
            .Where(x => x.CanRead);

        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(string) || IsScalarType(property.PropertyType))
            {
                continue;
            }

            var value = property.GetValue(instance);
            if (value is null)
            {
                continue;
            }

            ValidateObject(value, BuildPath(path, property.Name), errors);
        }
    }

    private static bool IsScalarType(Type type)
    {
        var targetType = Nullable.GetUnderlyingType(type) ?? type;

        return targetType.IsPrimitive
            || targetType.IsEnum
            || targetType == typeof(decimal)
            || targetType == typeof(DateTime)
            || targetType == typeof(DateTimeOffset)
            || targetType == typeof(TimeSpan)
            || targetType == typeof(Guid);
    }

    private static string BuildPath(string prefix, string memberName)
    {
        if (string.IsNullOrWhiteSpace(memberName))
        {
            return string.IsNullOrWhiteSpace(prefix) ? "settings" : prefix;
        }

        var normalized = char.ToLowerInvariant(memberName[0]) + memberName[1..];
        return string.IsNullOrWhiteSpace(prefix) ? normalized : $"{prefix}.{normalized}";
    }

    private static void AddError(
        IDictionary<string, List<string>> errors,
        string key,
        string message
    )
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors[key] = messages;
        }

        messages.Add(message);
    }
}
