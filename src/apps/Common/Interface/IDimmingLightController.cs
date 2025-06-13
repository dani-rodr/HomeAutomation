namespace HomeAutomation.apps.Common.Interface;

/// <summary>
/// Interface for controlling light dimming behavior in motion automations
/// Provides abstraction for testing and flexibility in dimming implementations
/// </summary>
public interface IDimmingLightController : IDisposable
{
    /// <summary>
    /// Handles motion detection event - typically turns on light at full brightness
    /// and cancels any pending turn-off operations
    /// </summary>
    /// <param name="light">The light entity to control</param>
    void OnMotionDetected(LightEntity light);

    /// <summary>
    /// Handles motion stopped event - implements dimming behavior based on sensor configuration
    /// May dim the light before turning off or turn off immediately based on settings
    /// </summary>
    /// <param name="light">The light entity to control</param>
    /// <returns>Task representing the async dimming operation</returns>
    Task OnMotionStoppedAsync(LightEntity light);

    /// <summary>
    /// Configures dimming parameters for brightness and delay timing
    /// </summary>
    /// <param name="brightnessPct">Brightness percentage during dimming phase (1-100)</param>
    /// <param name="delaySeconds">How long to keep light dimmed before turning off</param>
    void SetDimParameters(int brightnessPct, int delaySeconds);

    /// <summary>
    /// Sets the sensor active delay value that determines when dimming is enabled
    /// When sensor delay equals this value, dimming behavior is activated
    /// </summary>
    /// <param name="value">The sensor active delay value</param>
    void SetSensorActiveDelayValue(int value);
}
