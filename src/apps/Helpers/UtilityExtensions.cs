using System.Text.Json;

namespace HomeAutomation.apps.Helpers;

/// <summary>
/// Provides utility methods for time range validation and comparison.
/// Useful for determining if automations should be active during specific time periods.
/// </summary>
public static class TimeRange
{
    /// <summary>
    /// Determines if the current time falls within the specified hour range.
    /// Supports both same-day ranges (e.g., 9 AM to 5 PM) and overnight ranges (e.g., 10 PM to 6 AM).
    /// </summary>
    /// <param name="start">The start hour (0-23).</param>
    /// <param name="end">The end hour (0-23).</param>
    /// <returns>True if the current time is within the range, otherwise false.</returns>
    /// <example>
    /// <code>
    /// // Check if it's between 9 AM and 5 PM
    /// bool isWorkHours = TimeRange.IsCurrentTimeInBetween(9, 17);
    ///
    /// // Check if it's between 10 PM and 6 AM (overnight range)
    /// bool isNightTime = TimeRange.IsCurrentTimeInBetween(22, 6);
    /// </code>
    /// </example>
    public static bool IsCurrentTimeInBetween(int start, int end) =>
        IsTimeInBetween(DateTime.Now.TimeOfDay, start, end);

    /// <summary>
    /// Determines if the specified time falls within the specified hour range.
    /// Supports both same-day ranges and overnight ranges.
    /// </summary>
    /// <param name="now">The time to check.</param>
    /// <param name="start">The start hour (0-23).</param>
    /// <param name="end">The end hour (0-23).</param>
    /// <returns>True if the time is within the range, otherwise false.</returns>
    /// <remarks>
    /// For same-day ranges (start ≤ end): time must be between start and end inclusive.
    /// For overnight ranges (start > end): time must be after start OR before end.
    /// </remarks>
    public static bool IsTimeInBetween(TimeSpan now, int start, int end)
    {
        var startTime = TimeSpan.FromHours(start);
        var endTime = TimeSpan.FromHours(end);
        return start <= end ? now >= startTime && now <= endTime : now >= startTime || now <= endTime;
    }
}

/// <summary>
/// Provides extension methods for extracting and parsing attribute changes from state changes.
/// These methods handle JSON deserialization and type conversion for entity attributes.
/// </summary>
public static class StateChangeExtensions
{
    /// <summary>
    /// Extracts the old and new values of a specific attribute from a state change event.
    /// Handles both direct value types and JSON elements with proper deserialization.
    /// </summary>
    /// <typeparam name="T">The expected type of the attribute value.</typeparam>
    /// <param name="change">The state change event.</param>
    /// <param name="attributeName">The name of the attribute to extract.</param>
    /// <returns>A tuple containing the old and new attribute values, or default values if extraction fails.</returns>
    /// <example>
    /// <code>
    /// // Extract temperature attribute changes
    /// var (oldTemp, newTemp) = stateChange.GetAttributeChange&lt;double&gt;("temperature");
    /// if (oldTemp.HasValue && newTemp.HasValue)
    /// {
    ///     var tempDiff = newTemp.Value - oldTemp.Value;
    ///     // Process temperature change...
    /// }
    /// </code>
    /// </example>
    public static (T? Old, T? New) GetAttributeChange<T>(this StateChange change, string attributeName)
    {
        T? oldVal = TryGetAttributeValue<T>(change.Old?.Attributes, attributeName);
        T? newVal = TryGetAttributeValue<T>(change.New?.Attributes, attributeName);

        return (oldVal, newVal);
    }

    /// <summary>
    /// Attempts to extract and convert an attribute value to the specified type.
    /// Handles JSON elements by deserializing them, and performs type conversion for other value types.
    /// </summary>
    /// <typeparam name="T">The target type for the attribute value.</typeparam>
    /// <param name="attributes">The attributes dictionary to search.</param>
    /// <param name="key">The attribute key to look up.</param>
    /// <returns>The converted attribute value, or the default value of T if extraction/conversion fails.</returns>
    /// <remarks>
    /// This method safely handles:
    /// - Missing attributes (returns default)
    /// - JSON elements (deserializes using System.Text.Json)
    /// - Type conversion (using Convert.ChangeType)
    /// - All exceptions are caught and result in default values
    /// </remarks>
    private static T? TryGetAttributeValue<T>(IReadOnlyDictionary<string, object>? attributes, string key)
    {
        if (attributes == null || !attributes.TryGetValue(key, out var value))
            return default;

        if (value is JsonElement json)
        {
            try
            {
                return json.Deserialize<T>();
            }
            catch
            {
                return default;
            }
        }

        try
        {
            return (T?)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }
}
