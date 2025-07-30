namespace HomeAutomation.apps.Helpers;

/// <summary>
/// Provides a testable scheduler abstraction for reactive extensions.
/// Allows tests to inject TestScheduler while production code uses Scheduler.Default.
/// </summary>
/// <remarks>
/// This class enables time-dependent reactive operations to be testable by allowing
/// tests to control time advancement through TestScheduler injection. The thread-safe
/// implementation ensures proper test isolation when running tests in parallel.
///
/// Usage in tests:
/// - Set SchedulerProvider.Current to TestScheduler in test setup
/// - Reset SchedulerProvider.Current to null in test cleanup
/// - Use TestScheduler.AdvanceBy() to control time advancement
/// </remarks>
public static class SchedulerProvider
{
    private static readonly ThreadLocal<IScheduler?> _current = new();

    /// <summary>
    /// Gets or sets the current scheduler for reactive operations.
    /// Returns Scheduler.Default when no custom scheduler is set.
    /// </summary>
    /// <remarks>
    /// This property is thread-safe and allows each test thread to have its own scheduler context.
    /// Production code will always get Scheduler.Default unless explicitly overridden.
    /// </remarks>
    public static IScheduler Current
    {
        get => _current.Value ?? Scheduler.Default;
        set => _current.Value = value;
    }

    /// <summary>
    /// Resets the current scheduler to null, causing it to fall back to Scheduler.Default.
    /// </summary>
    /// <remarks>
    /// This method should be called in test cleanup to ensure proper isolation between tests.
    /// After calling Reset(), Current will return Scheduler.Default.
    /// </remarks>
    public static void Reset() => _current.Value = null;

    /// <summary>
    /// Disposes the ThreadLocal storage for the current scheduler.
    /// </summary>
    /// <remarks>
    /// This method should be called during application shutdown to properly clean up resources.
    /// In practice, this is rarely needed since the ThreadLocal will be garbage collected.
    /// </remarks>
    public static void Dispose() => _current.Dispose();
}
