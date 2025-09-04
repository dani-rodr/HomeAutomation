using System.Linq;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class SwitchEntityExtensions
{
    /// <summary>
    /// Creates an observable that detects double-click patterns on switch entities.
    /// </summary>
    /// <param name="source">The source observable of switch state changes.</param>
    /// <param name="timeout">The maximum time in seconds between clicks to be considered a double-click.</param>
    /// <returns>An observable that emits when a double-click pattern is detected.</returns>
    /// <remarks>
    /// This method uses a sliding window approach to detect two consecutive state changes
    /// within the specified timeout period. Useful for implementing double-tap switch automations.
    /// </remarks>
    public static IObservable<
        IList<StateChange<SwitchEntity, EntityState<SwitchAttributes>>>
    > OnDoubleClick(this SwitchEntity entity, int timeout)
    {
        const int maxBufferSize = 2;

        return entity
            .StateChanges()
            .Timestamp(SchedulerProvider.Current)
            .Buffer(maxBufferSize, 1) // sliding window of 2 consecutive changes
            .Where(pair =>
                pair.Count == maxBufferSize
                && (pair[1].Timestamp - pair[0].Timestamp) <= TimeSpan.FromSeconds(timeout)
            )
            .Select(pair => pair.Select(x => x.Value).ToList());
    }
}
