using System.Text.Json;
using HomeAutomation.apps.Common.Services;
using Microsoft.Reactive.Testing;

namespace HomeAutomation.Tests.Common.Services;

/// <summary>
/// Comprehensive tests for DimmingLightController service
/// Tests dimming logic, async behavior, configuration, and cancellation scenarios
/// </summary>
public class DimmingLightControllerTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly NumberEntity _sensorDelay;
    private readonly Mock<ILogger<DimmingLightController>> _mockLogger;
    private readonly TestScheduler _testScheduler = new();
    private readonly LightEntity _light;
    private readonly DimmingLightController _controller;

    public DimmingLightControllerTests()
    {
        _mockHaContext = new MockHaContext();
        _sensorDelay = new NumberEntity(_mockHaContext, "number.test_sensor_delay");
        _light = new LightEntity(_mockHaContext, "light.test_light");
        _mockLogger = new Mock<ILogger<DimmingLightController>>();
        _controller = new DimmingLightController(_sensorDelay, _testScheduler, _mockLogger.Object);
    }

    [Fact]
    public void OnMotionDetected_Should_TurnOnLightAtFullBrightness()
    {
        // Act
        _controller.OnMotionDetected(_light);

        // Assert - Should turn on light at 100% brightness
        _mockHaContext.ShouldHaveCalledLightTurnOn(_light.EntityId);

        // Verify it was called with full brightness data
        var turnOnCall = _mockHaContext
            .GetServiceCalls("light")
            .FirstOrDefault(call =>
                call.Service == "turn_on"
                && call.Target?.EntityIds?.Contains(_light.EntityId) == true
            );

        turnOnCall.Should().NotBeNull();
        GetBrightnessFromServiceCall(turnOnCall!).Should().Be(100);
    }

    [Fact]
    public async Task OnMotionDetected_Should_CancelPendingTurnOff()
    {
        // Arrange - Set up dimming scenario first
        _mockHaContext.SetEntityState(_sensorDelay.EntityId, "5"); // Match default active delay
        _controller.SetSensorActiveDelayValue(5);
        _controller.SetDimParameters(brightnessPct: 80, delaySeconds: 2); // Shorter delay for test

        // Start a dimming operation
        var dimmingTask = _controller.OnMotionStoppedAsync(_light);
        _testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks); // Let it start

        // Act - Motion detected should cancel the dimming
        _controller.OnMotionDetected(_light);

        // Assert - The dimming task should complete quickly (was cancelled)
        var completed = await Task.WhenAny(dimmingTask, Task.Delay(TimeSpan.FromSeconds(1)));
        completed.Should().Be(dimmingTask);

        _mockHaContext.ShouldHaveCalledLightTurnOn(_light.EntityId);
    }

    [Fact]
    public async Task OnMotionStoppedAsync_WithDimmingDisabled_Should_TurnOffImmediately()
    {
        // Arrange - Set sensor delay to value that disables dimming
        _mockHaContext.SetEntityState(_sensorDelay.EntityId, "10"); // Different from default active delay (5)

        // Act
        var task = _controller.OnMotionStoppedAsync(_light);
        _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1.1).Ticks);

        await task;
        // Assert - Should turn off immediately, no dimming
        _mockHaContext.ShouldHaveCalledLightTurnOff(_light.EntityId);

        // Should not have any turn_on calls (no dimming)
        var lightCalls = _mockHaContext.GetServiceCalls("light").ToList();
        var turnOnCalls = lightCalls.Where(call => call.Service == "turn_on").ToList();
        turnOnCalls.Should().BeEmpty("Should not dim when dimming is disabled");
    }

    [Fact]
    public async Task OnMotionStoppedAsync_WithDimmingEnabled_Should_DimThenTurnOff()
    {
        // Arrange - Set sensor delay to match active delay (enables dimming)
        _mockHaContext.SetEntityState(_sensorDelay.EntityId, "5"); // Match default active delay
        _controller.SetSensorActiveDelayValue(5);
        _controller.SetDimParameters(brightnessPct: 60, delaySeconds: 1); // Short delay for test

        // Act
        var task = _controller.OnMotionStoppedAsync(_light);
        _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1.1).Ticks);

        await task;

        _mockHaContext.ShouldHaveCalledLightTurnOn(_light.EntityId);
        _mockHaContext.ShouldHaveCalledLightTurnOff(_light.EntityId);

        var dimCall = _mockHaContext
            .GetServiceCalls("light")
            .FirstOrDefault(c =>
                c.Service == "turn_on" && c.Target?.EntityIds?.Contains(_light.EntityId) == true
            );

        dimCall.Should().NotBeNull();
        // Verify dimming brightness
        GetBrightnessFromServiceCall(dimCall!).Should().Be(60);
    }

    [Fact]
    public async Task SetDimParameters_Should_UpdateBrightnessAndDelay()
    {
        // Arrange - Set up dimming enabled scenario
        _mockHaContext.SetEntityState(_sensorDelay.EntityId, "5");
        _controller.SetSensorActiveDelayValue(5);

        // Act - Configure custom dimming parameters
        _controller.SetDimParameters(brightnessPct: 75, delaySeconds: 1);

        var task = _controller.OnMotionStoppedAsync(_light);
        _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1.1).Ticks);

        await task;

        _mockHaContext.ShouldHaveCalledLightTurnOn(_light.EntityId);

        var turnOnCall = _mockHaContext
            .GetServiceCalls("light")
            .FirstOrDefault(c =>
                c.Service == "turn_on" && c.Target?.EntityIds?.Contains(_light.EntityId) == true
            );

        turnOnCall.Should().NotBeNull();
        GetBrightnessFromServiceCall(turnOnCall!).Should().Be(75);
    }

    [Fact]
    public async Task SetSensorActiveDelayValue_Should_UpdateDimmingTrigger()
    {
        // Arrange
        _mockHaContext.SetEntityState(_sensorDelay.EntityId, "25");
        _controller.SetDimParameters(brightnessPct: 80, delaySeconds: 1);
        // Act - Set active delay to match sensor state
        _controller.SetSensorActiveDelayValue(25);

        var task = _controller.OnMotionStoppedAsync(_light);
        _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1.1).Ticks);

        await task;

        _mockHaContext.ShouldHaveCalledLightTurnOn(_light.EntityId);
    }

    [Fact]
    public async Task OnMotionStoppedAsync_WhenCancelled_Should_NotTurnOffLight()
    {
        // Arrange - Set up dimming scenario
        _mockHaContext.SetEntityState(_sensorDelay.EntityId, "5");
        _controller.SetSensorActiveDelayValue(5);
        _controller.SetDimParameters(brightnessPct: 80, delaySeconds: 2); // Shorter delay for test

        // Act - Start dimming and quickly cancel with motion
        var task = _controller.OnMotionStoppedAsync(_light);
        _testScheduler.AdvanceBy(TimeSpan.FromSeconds(2.1).Ticks);

        await task;

        _mockHaContext.ShouldHaveCalledLightTurnOn(_light.EntityId);
        _mockHaContext.ShouldHaveCalledLightTurnOff(_light.EntityId);
    }

    [Fact]
    public async Task SensorDelayState_Null_Should_TreatAsZero()
    {
        // Arrange - Sensor returns null state (entity not set)
        _controller.SetDimParameters(brightnessPct: 80, delaySeconds: 1);
        _controller.SetSensorActiveDelayValue(0); // Should match null (treated as 0)

        // Act
        var task = _controller.OnMotionStoppedAsync(_light);
        _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1.1).Ticks);

        await task;

        _mockHaContext.ShouldHaveCalledLightTurnOn(_light.EntityId);
    }

    [Fact]
    public async Task DefaultConfiguration_Should_WorkCorrectly()
    {
        // Arrange - Use (sensor active delay: 1, brightness: 80%, delay: 5s)
        _controller.SetDimParameters(brightnessPct: 80, delaySeconds: 1);
        _mockHaContext.SetEntityState(_sensorDelay.EntityId, "5");

        // Act - Should use default configuration
        var task = _controller.OnMotionStoppedAsync(_light);
        _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1.1).Ticks); // simulate time

        await task;

        _mockHaContext.ShouldHaveCalledLightTurnOn(_light.EntityId);

        var turnOnCall = _mockHaContext
            .GetServiceCalls("light")
            .FirstOrDefault(c =>
                c.Service == "turn_on" && c.Target?.EntityIds?.Contains(_light.EntityId) == true
            );

        turnOnCall.Should().NotBeNull();
        GetBrightnessFromServiceCall(turnOnCall!).Should().Be(80);
    }

    /// <summary>
    /// Helper method to extract brightness percentage from service call data
    /// </summary>
    private static int GetBrightnessFromServiceCall(ServiceCall serviceCall)
    {
        // Handle LightTurnOnParameters object
        if (
            serviceCall.Data is LightTurnOnParameters lightParams
            && lightParams.BrightnessPct.HasValue
        )
        {
            return (int)lightParams.BrightnessPct.Value;
        }

        // Handle JSON data (fallback)
        if (
            serviceCall.Data is JsonElement dataElement
            && dataElement.ValueKind == JsonValueKind.Object
            && dataElement.TryGetProperty("brightness_pct", out var brightnessProperty)
        )
        {
            return brightnessProperty.GetInt32();
        }

        throw new InvalidOperationException(
            $"Service call data does not contain brightness_pct: {serviceCall.Data}"
        );
    }

    public void Dispose()
    {
        _controller?.Dispose();
        _mockHaContext?.Dispose();
    }
}
