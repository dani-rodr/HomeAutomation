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
    /// For same-day ranges (start ≤ end): time must be between start (inclusive) and end (exclusive).
    /// For overnight ranges (start > end): time must be after start (inclusive) OR before end (exclusive).
    /// </remarks>
    public static bool IsTimeInBetween(TimeSpan now, int start, int end)
    {
        var startTime = TimeSpan.FromHours(start);
        var endTime = TimeSpan.FromHours(end);
        return start <= end ? now >= startTime && now < endTime : now >= startTime || now < endTime;
    }
}
