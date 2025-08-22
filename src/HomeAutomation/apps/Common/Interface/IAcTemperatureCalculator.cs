namespace HomeAutomation.apps.Common.Interface;

/// <summary>
/// Provides temperature calculation for AC settings based on occupancy and environmental factors.
/// </summary>
public interface IAcTemperatureCalculator
{
    /// <summary>
    /// Calculates the appropriate temperature setting based on the provided settings and conditions.
    /// </summary>
    /// <param name="settings">The AC settings configuration</param>
    /// <param name="isOccupied">Whether the space is currently occupied</param>
    /// <param name="isDoorOpen">Whether the door is currently open</param>
    /// <returns>The calculated temperature in Celsius</returns>
    int CalculateTemperature(AcSettings settings, bool isOccupied, bool isDoorOpen);
}
