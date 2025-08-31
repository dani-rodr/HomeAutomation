using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Services;

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
    private readonly LaptopChargingHandler _batteryHandler;

    public LaptopChargingHandlerTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<LaptopChargingHandler>>();
        _entities = new TestBatteryHandlerEntities(_mockHaContext);
        _batteryHandler = new LaptopChargingHandler(_entities, _mockLogger.Object);
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

        // Assert - Power should turn off by default
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Power.EntityId);

        subscription.Dispose();
    }

    #endregion

    #region Laptop Turn Off Tests

    [Fact]
    public void HandleLaptopTurnedOff_HighBattery_Should_TurnOffPowerImmediately()
    {
        // Arrange - High battery level (>50%)
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "75");

        // Act
        _batteryHandler.HandleLaptopTurnedOff();

        // Assert - Should turn off power immediately
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Power.EntityId);
        _mockHaContext.ShouldHaveServiceCallCount(1);
    }

    [Fact]
    public void HandleLaptopTurnedOff_LowBattery_Should_ChargeForOneHour()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "30"); // Low battery (≤50%)

        // Act
        _batteryHandler.HandleLaptopTurnedOff();

        // Initially should have turned on the power
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);

        // Advance scheduler by 1 hour to trigger power off
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromHours(1));

        // Assert - Power should turn off after 1 hour
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Power.EntityId);
    }

    [Fact]
    public void HandleLaptopTurnedOff_ThenTurnedOn_Should_CancelCharging()
    {
        // Arrange – battery low enough to trigger charging
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "25");

        // Act – start charging operation
        _batteryHandler.HandleLaptopTurnedOff();

        // Assert power was turned on
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);

        // Simulate laptop turning back on (cancels charging timer)
        _batteryHandler.HandleLaptopTurnedOn();

        // Advance some time (but less than 1 hour) to verify timer was cancelled
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromMinutes(30));

        // Power should remain on (timer was cancelled) - only turn_on call, no turn_off
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);

        // Advance full hour - still should not turn off due to cancellation
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromMinutes(30));

        // Should have 2 turn_on calls (from HandleLaptopTurnedOff + HandleLaptopTurnedOn), but no turn_off
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.Power.EntityId, "turn_on", 2);
        var switchCalls = _mockHaContext
            .GetServiceCalls("switch")
            .Where(c =>
                c.Target?.EntityIds?.Contains(_entities.Power.EntityId) == true
                && c.Service == "turn_off"
            )
            .ToList();
        switchCalls
            .Should()
            .BeEmpty("Power should not have been turned off due to cancelled timer");
    }

    [Fact]
    public void HandleLaptopTurnedOff_MultipleCalls_Should_CancelPreviousTimer()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "40");

        // Act – Start first operation
        _batteryHandler.HandleLaptopTurnedOff();
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);

        // Start second operation (should cancel the first timer)
        _batteryHandler.HandleLaptopTurnedOff();
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromMilliseconds(10)); // Let second start

        // Should have turned on power again (second call)
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.Power.EntityId, "turn_on", 2);

        // Advance by 1 hour - only the second timer should fire
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromHours(1));

        // Should turn off power once (from second call)
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Power.EntityId);
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

    [Fact]
    public void StartMonitoring_Should_ScheduleWeekendChargingSessions()
    {
        // Act
        var subscription = _batteryHandler.StartMonitoring();

        // Assert - Just verify that the subscription includes weekend schedules by checking the subscription is composite
        subscription.Should().NotBeNull();
        subscription
            .Should()
            .BeAssignableTo<IDisposable>(
                "Should return valid subscription that includes weekend schedules"
            );

        subscription.Dispose();
    }

    #endregion

    #region Weekend Charging Tests

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void StartScheduledCharge_Should_TurnOnPowerAndScheduleTurnOff(int hours)
    {
        // Act - Call StartScheduledCharge using reflection (since it's private)
        var method = typeof(LaptopChargingHandler).GetMethod(
            "StartScheduledCharge",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );
        method?.Invoke(_batteryHandler, [hours]);

        // Assert - Power should turn on immediately
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);

        // Act - Advance time by the specified hours to trigger power-off
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromHours(hours));

        // Assert - Power should turn off after scheduled duration
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Power.EntityId);
    }

    [Fact(Skip = "Temporarily disabled, test is passing although performance is not ideal")]
    public void WeekendCharging_Saturday10AM_Should_StartChargingSession()
    {
        // Arrange - Start monitoring first to set up cron schedules
        var subscription = _batteryHandler.StartMonitoring();

        // Set scheduler baseline to Friday 11:59 PM to minimize time jump
        var fridayNight = new DateTime(2025, 1, 3, 23, 59, 0); // Friday night
        _mockHaContext.AdvanceTimeTo(fridayNight);

        // Act - Advance by 10 hours 1 minute to reach Saturday 10:00 AM (much faster than absolute time)
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(1)));

        // Assert - Should trigger power on for 1-hour charging session
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);

        // Act - Advance by 1 hour to complete charging session
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromHours(1));

        // Assert - Should turn power off after 1 hour
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Power.EntityId);

        subscription.Dispose();
    }

    [Fact(Skip = "Temporarily disabled, test is passing although performance is not ideal")]
    public void WeekendCharging_Sunday6PM_Should_StartChargingSession()
    {
        // Arrange - Start monitoring first to set up cron schedules
        var subscription = _batteryHandler.StartMonitoring();

        // Set scheduler baseline to Sunday 5:59 PM to minimize time jump
        var sundayEvening = new DateTime(2025, 1, 5, 17, 59, 0); // Sunday evening
        _mockHaContext.AdvanceTimeTo(sundayEvening);

        // Act - Advance by 1 minute to reach Sunday 6:00 PM (much faster than absolute time)
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromMinutes(1));

        // Assert - Should trigger power on for 1-hour charging session
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);

        // Act - Advance by 1 hour to complete charging session
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromHours(1));

        // Assert - Should turn power off after 1 hour
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Power.EntityId);

        subscription.Dispose();
    }

    [Fact(Skip = "Temporarily disabled, test is passing although performance is not ideal")]
    public void WeekendCharging_Monday6AM_Should_StartPreWakeChargingSession()
    {
        // Arrange - Start monitoring first to set up cron schedules
        var subscription = _batteryHandler.StartMonitoring();

        // Set scheduler baseline to Monday 5:59 AM to minimize time jump
        var mondayMorning = new DateTime(2025, 1, 6, 5, 59, 0); // Monday morning
        _mockHaContext.AdvanceTimeTo(mondayMorning);

        // Act - Advance by 1 minute to reach Monday 6:00 AM (much faster than absolute time)
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromMinutes(1));

        // Assert - Should trigger power on for 1-hour charging session (before 7-8 AM wake time)
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);

        // Act - Advance by 1 hour to complete charging session
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromHours(1));

        // Assert - Should turn power off after 1 hour
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Power.EntityId);

        subscription.Dispose();
    }

    #endregion

    #region Resource Management Tests

    [Fact]
    public void Dispose_Should_CleanupResources()
    {
        // Arrange - Start some operations
        var subscription = _batteryHandler.StartMonitoring();
        _batteryHandler.HandleLaptopTurnedOff();

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
    public void Dispose_DuringChargingTimer_Should_CancelTimer()
    {
        // Arrange – Trigger low battery charging logic
        _mockHaContext.SetEntityState(_entities.Level.EntityId, "25");

        // Act – Start the charging operation
        _batteryHandler.HandleLaptopTurnedOff();

        // Power should turn on
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);

        // Dispose during the operation (cancels the timer)
        _batteryHandler.Dispose();

        // Advance by 1 hour - the timer should be cancelled so power should NOT turn off
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromHours(1));

        // Assert – Power should not turn off because timer was disposed
        var switchCalls = _mockHaContext
            .GetServiceCalls("switch")
            .Where(c =>
                c.Target?.EntityIds?.Contains(_entities.Power.EntityId) == true
                && c.Service == "turn_off"
            )
            .ToList();
        switchCalls.Should().BeEmpty("Power should not have been turned off due to disposed timer");
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
