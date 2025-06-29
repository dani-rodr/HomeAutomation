using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Services;
using Microsoft.Reactive.Testing;

namespace HomeAutomation.Tests.Common.Services;

/// <summary>
/// Comprehensive tests for LaptopBatteryHandler service covering battery management,
/// charging logic, async operations, cancellation scenarios, and resource disposal.
/// Tests battery level thresholds, force charging, and laptop power state coordination.
/// </summary>
public class LaptopChargingHandlerTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<ILogger<LaptopChargingHandler>> _mockLogger;
    private readonly TestBatteryHandlerEntities _entities;
    private readonly TestScheduler _testScheduler = new();
    private readonly LaptopChargingHandler _batteryHandler;

    public LaptopChargingHandlerTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<LaptopChargingHandler>>();
        _entities = new TestBatteryHandlerEntities(_mockHaContext);
        _batteryHandler = new LaptopChargingHandler(_entities, _testScheduler);
    }

    #region Constructor & Initialization Tests

    [Fact]
    public void Constructor_Should_InitializeWithCorrectEntities()
    {
        // Assert - Verify initialization doesn't throw and entities are accessible
        _entities.Level.EntityId.Should().Be("sensor.laptop_battery_level");
        _entities.Power.EntityId.Should().Be("switch.laptop_power");
        _batteryHandler.Should().NotBeNull();
    }

    [Fact]
    public void BatteryLevel_WhenSensorHasValue_Should_ReturnSensorValue()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "75");

        // Act & Assert
        _batteryHandler.BatteryLevel.Should().Be(75);
    }

    [Fact]
    public void BatteryLevel_WhenSensorIsNull_Should_ReturnLastKnownValue()
    {
        // Arrange - First set a value and trigger charging logic to cache it
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "65");
        _batteryHandler.HandleLaptopTurnedOn(); // This will trigger ApplyChargingLogic and cache the value

        // Clear sensor value
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "");

        // Act & Assert - Should return last known value
        _batteryHandler.BatteryLevel.Should().Be(65);
    }

    #endregion

    #region Charging Logic Tests

    [Fact]
    public void HandleLaptopTurnedOn_HighBattery_Should_TurnOffPower()
    {
        // Arrange - High battery level (≥80%)
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "85");

        // Act
        _batteryHandler.HandleLaptopTurnedOn();

        // Assert - Should turn off power to prevent overcharging
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Power.EntityId);
    }

    [Fact]
    public void HandleLaptopTurnedOn_LowBattery_Should_TurnOnPower()
    {
        // Arrange - Low battery level (≤20%)
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "15");

        // Act
        _batteryHandler.HandleLaptopTurnedOn();

        // Assert - Should turn on power for charging
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);
    }

    [Fact]
    public void HandleLaptopTurnedOn_MediumBattery_Should_TurnOnPowerDueToForceCharge()
    {
        // Arrange - Medium battery level (21-79%)
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "50");

        // Act
        _batteryHandler.HandleLaptopTurnedOn();

        // Assert - Should turn on power due to forceCharge=true
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);
    }

    [Theory]
    [InlineData(80, false)] // High battery - turn off
    [InlineData(90, false)] // Very high battery - turn off
    [InlineData(20, true)] // Low battery threshold - turn on
    [InlineData(15, true)] // Very low battery - turn on
    [InlineData(10, true)] // Critical battery - turn on
    public void ApplyChargingLogic_BatteryThresholds_Should_ControlPowerCorrectly(
        int batteryLevel,
        bool shouldTurnOn
    )
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Level.EntityId, batteryLevel.ToString());

        // Act - Trigger charging logic through monitoring
        var subscription = _batteryHandler.StartMonitoring();
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Level,
            "0",
            batteryLevel.ToString()
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert
        if (shouldTurnOn)
        {
            _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);
        }
        else
        {
            _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Power.EntityId);
        }

        subscription.Dispose();
    }

    [Fact]
    public void ApplyChargingLogic_MediumBatteryWithoutForce_Should_NotChangeState()
    {
        // Arrange - Medium battery level (between thresholds)
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "50");

        // Act - Monitor without force charge
        var subscription = _batteryHandler.StartMonitoring();
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.Level, "60", "50");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - No power state changes should occur
        _mockHaContext.ShouldHaveServiceCallCount(0);

        subscription.Dispose();
    }

    #endregion

    #region Laptop Turn Off Async Tests

    [Fact]
    public async Task HandleLaptopTurnedOffAsync_HighBattery_Should_TurnOffPowerImmediately()
    {
        // Arrange - High battery level (>50%)
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "75");

        // Act
        await _batteryHandler.HandleLaptopTurnedOffAsync();

        // Assert - Should turn off power immediately
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Power.EntityId);
        _mockHaContext.ShouldHaveServiceCallCount(1);
    }

    [Fact]
    public void HandleLaptopTurnedOffAsync_LowBattery_Should_ChargeForOneHour()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "30"); // Low battery (≤50%)

        var task = _batteryHandler.HandleLaptopTurnedOffAsync();

        // Initially should have turned on the power
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);

        // Advance scheduler by 1 hour
        _testScheduler.AdvanceBy(TimeSpan.FromHours(1).Ticks);

        // Act & Assert
        task.IsCompleted.Should().BeTrue("operation should complete after 1 hour");
    }

    [Fact]
    public async Task HandleLaptopTurnedOffAsync_Cancelled_Should_StopCharging()
    {
        // Arrange – battery low enough to trigger charging
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "25");

        // Act – start async charging operation
        var turnOffTask = _batteryHandler.HandleLaptopTurnedOffAsync();

        // Assert power was turned on
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);

        // Simulate laptop turning back on (cancels charging)
        _batteryHandler.HandleLaptopTurnedOn();

        // Advance scheduler to allow cancellation to process
        _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

        // Await task completion and ensure no exceptions are thrown
        await turnOffTask;

        // Assert – task completed after cancellation
        turnOffTask.IsCompleted.Should().BeTrue("Charging task should complete when cancelled");
    }

    [Fact]
    public async Task HandleLaptopTurnedOffAsync_MultipleCalls_Should_CancelPreviousOperation()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "40");

        // Act – Start first operation
        var firstTask = _batteryHandler.HandleLaptopTurnedOffAsync();
        _testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(10).Ticks); // Let it start

        // Start second operation (should cancel the first one)
        var secondTask = _batteryHandler.HandleLaptopTurnedOffAsync();
        _testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(10).Ticks); // Let second start

        // Cleanup: cancel both
        _batteryHandler.Dispose();
        _testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(10).Ticks); // Allow cancel to propagate

        // Assert – Both tasks should complete without error
        try
        {
            await firstTask;
            await secondTask;
        }
        catch (ObjectDisposedException)
        {
            // Expected during disposal
        }

        // Assert – Power was turned on at least once
        _mockHaContext
            .GetServiceCalls("switch")
            .Count(call => call.Service == "turn_on")
            .Should()
            .BeGreaterThanOrEqualTo(1, "power should be turned on during at least one operation");
    }

    #endregion

    #region Monitoring Tests

    [Fact]
    public void StartMonitoring_Should_ReturnSubscription()
    {
        // Act
        var subscription = _batteryHandler.StartMonitoring();

        // Assert
        subscription.Should().NotBeNull();
        subscription.Should().BeAssignableTo<IDisposable>();

        subscription.Dispose();
    }

    [Fact]
    public void StartMonitoring_BatteryLevelChanges_Should_TriggerChargingLogic()
    {
        // Arrange
        var subscription = _batteryHandler.StartMonitoring();

        // Act - Simulate battery level change to low threshold
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "19");
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.Level, "50", "19");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should trigger power on due to low battery
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);

        subscription.Dispose();
    }

    [Fact]
    public void StartMonitoring_BatteryLevelCaching_Should_UpdateLastKnownValue()
    {
        // Arrange
        var subscription = _batteryHandler.StartMonitoring();

        // Act - Trigger state change
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "42");
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.Level, "30", "42");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Clear sensor value to test caching
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "");

        // Assert - Should return cached value
        _batteryHandler.BatteryLevel.Should().Be(42);

        subscription.Dispose();
    }

    #endregion

    #region Resource Management Tests

    [Fact]
    public void Dispose_Should_CleanupResources()
    {
        // Arrange - Start some operations
        var subscription = _batteryHandler.StartMonitoring();
        _ = _batteryHandler.HandleLaptopTurnedOffAsync(); // Don't await

        // Act
        _batteryHandler.Dispose();

        // Assert - Should not throw exceptions
        Assert.True(true, "Disposal completed without exceptions");

        subscription.Dispose();
    }

    [Fact]
    public void Dispose_Multiple_Should_NotThrow()
    {
        // Act - Multiple disposal calls
        _batteryHandler.Dispose();
        _batteryHandler.Dispose();

        // Assert - Should handle multiple disposal gracefully
        Assert.True(true, "Multiple disposal calls handled correctly");
    }

    [Fact]
    public async Task Dispose_DuringAsyncOperation_Should_CancelOperation()
    {
        // Arrange – Trigger low battery charging logic
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "25");

        // Act – Start the async operation
        var task = _batteryHandler.HandleLaptopTurnedOffAsync();

        // Simulate time for the task to start
        _testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(10).Ticks);

        // Dispose during the operation
        _batteryHandler.Dispose();

        // Advance time to allow cancellation to propagate
        _testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(10).Ticks);

        // Assert – The task should complete without hanging
        try
        {
            await task;
        }
        catch (ObjectDisposedException)
        {
            // Expected when disposing during operation
        }

        task.IsCompleted.Should().BeTrue("Async operation should complete after disposal");
    }

    #endregion

    #region Edge Cases & Error Handling

    [Fact]
    public void BatteryLevel_InvalidSensorValue_Should_UseLastKnownValue()
    {
        // Arrange - Set initial valid value and cache it
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "60");
        _batteryHandler.HandleLaptopTurnedOn(); // Cache the value through ApplyChargingLogic

        // Act - Set invalid value
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "invalid");

        // Assert - Should fall back to last known value
        _batteryHandler.BatteryLevel.Should().Be(60);
    }

    [Fact]
    public void StartMonitoring_MultipleSubscriptions_Should_WorkIndependently()
    {
        // Act - Create multiple subscriptions
        var subscription1 = _batteryHandler.StartMonitoring();
        var subscription2 = _batteryHandler.StartMonitoring();

        // Assert - Both should be valid
        subscription1.Should().NotBeNull();
        subscription2.Should().NotBeNull();

        // Cleanup
        subscription1.Dispose();
        subscription2.Dispose();
    }

    [Fact]
    public void HandleLaptopTurnedOn_RapidCalls_Should_HandleCorrectly()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "50");

        // Act - Rapid calls
        _batteryHandler.HandleLaptopTurnedOn();
        _batteryHandler.HandleLaptopTurnedOn();
        _batteryHandler.HandleLaptopTurnedOn();

        // Assert - Should handle multiple calls without issues
        _mockHaContext
            .GetServiceCalls("switch")
            .Count(call => call.Service == "turn_on")
            .Should()
            .Be(3, "Each call should trigger power on");
    }

    #endregion

    public void Dispose()
    {
        _batteryHandler?.Dispose();
        _mockHaContext?.Dispose();
    }

    /// <summary>
    /// Test implementation of IBatteryHandlerEntities for unit testing
    /// </summary>
    private class TestBatteryHandlerEntities(IHaContext haContext) : IChargingHandlerEntities
    {
        public NumericSensorEntity Level { get; } = new(haContext, "sensor.laptop_battery_level");
        public SwitchEntity Power { get; } = new(haContext, "switch.laptop_power");
    }
}
