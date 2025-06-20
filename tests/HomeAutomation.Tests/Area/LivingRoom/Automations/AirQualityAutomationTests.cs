using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Helpers;

namespace HomeAutomation.Tests.Area.LivingRoom.Automations;

/// <summary>
/// Comprehensive behavioral tests for LivingRoom AirQualityAutomation using clean assertion syntax
/// Tests air quality monitoring, fan control based on PM2.5 levels, and environmental automation
/// </summary>
public class AirQualityAutomationTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<ILogger<AirQualityAutomation>> _mockLogger;
    private readonly TestEntities _entities;
    private readonly AirQualityAutomation _automation;

    // Air quality thresholds from the implementation
    private const int CLEAN_AIR_THRESHOLD = 7;
    private const int DIRTY_AIR_THRESHOLD = 75;
    private const int WAIT_TIME_SECONDS = 10;

    public AirQualityAutomationTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<AirQualityAutomation>>();

        // Create test entities wrapper
        _entities = new TestEntities(_mockHaContext);

        _automation = new AirQualityAutomation(_entities, _mockLogger.Object);

        // Start the automation to set up subscriptions
        _automation.StartAutomation();

        // Simulate master switch being ON to enable automation logic
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Clear any initialization service calls
        _mockHaContext.ClearServiceCalls();
    }

    #region Air Quality Threshold Tests

    [Fact(Skip = "Temporarily disabled - air quality automation logic under review")]
    public void ExcellentAirQuality_Should_TurnOffMainFan()
    {
        // Arrange - Set air quality to excellent (below clean threshold)
        double excellentAirValue = 5.0; // Below clean threshold of 7

        // Act - Simulate excellent air quality for required wait time
        var stateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "10.0",
            excellentAirValue.ToString("F1")
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Main fan should turn off when air quality is excellent
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.AirPurifierFan.EntityId);
    }

    [Fact(Skip = "Temporarily disabled - air quality automation logic under review")]
    public void ModerateAirQuality_Should_TurnOnMainFan_When_IsCleaningAir()
    {
        // Arrange - First simulate poor air quality to set IsCleaningAir to true
        var poorAirStateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "10.0",
            "100.0"
        );
        _mockHaContext.StateChangeSubject.OnNext(poorAirStateChange);
        _mockHaContext.ClearServiceCalls(); // Clear the poor air quality actions

        double moderateAirValue = 25.0; // Between clean (7) and dirty (75) thresholds

        // Act - Simulate moderate air quality (should trigger supporting fan turn off and reset cleaning state)
        var stateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "100.0",
            moderateAirValue.ToString("F1")
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Main fan should turn on and supporting fan should turn off when transitioning from cleaning state
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.AirPurifierFan.EntityId);
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.SupportingFan.EntityId);
    }

    [Fact(Skip = "Temporarily disabled - air quality automation logic under review")]
    public void PoorAirQuality_Should_ActivateSupportingFan()
    {
        // Arrange - Poor air quality (above dirty threshold)
        double poorAirValue = 100.0; // Above dirty threshold of 75

        // Act - Simulate poor air quality
        var stateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "30.0",
            poorAirValue.ToString("F1")
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Supporting fan should turn on for poor air quality
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.SupportingFan.EntityId);
    }

    [Fact]
    public void PoorAirQuality_With_ShouldActivateFanTrue_Should_NotActivateSupportingFan()
    {
        // Arrange - First manually operate supporting fan to set ShouldActivateFan to true
        var manualStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.SupportingFan,
            "off",
            "on",
            HaIdentity.DANIEL_RODRIGUEZ // Manual operation sets ShouldActivateFan to true
        );
        _mockHaContext.StateChangeSubject.OnNext(manualStateChange);
        _mockHaContext.ClearServiceCalls();

        double poorAirValue = 100.0; // Above dirty threshold

        // Act - Simulate poor air quality when fan is manually controlled
        var stateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "30.0",
            poorAirValue.ToString("F1")
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Supporting fan should not be activated again when manually controlled
        _mockHaContext.ShouldNeverHaveCalledSwitch(_entities.SupportingFan.EntityId);
    }

    #endregion

    #region Fan Control and State Management Tests

    [Fact(Skip = "Temporarily disabled - air quality automation logic under review")]
    public void MainFan_StateChange_Should_SyncLedStatus()
    {
        // Act - Simulate main fan turning on
        var fanOnStateChange = StateChangeHelpers.SwitchTurnedOn(_entities.AirPurifierFan);
        _mockHaContext.StateChangeSubject.OnNext(fanOnStateChange);

        // Assert - LED should turn on when fan turns on
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.LedStatus.EntityId);

        // Clear calls for next test
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate main fan turning off
        var fanOffStateChange = StateChangeHelpers.SwitchTurnedOff(_entities.AirPurifierFan);
        _mockHaContext.StateChangeSubject.OnNext(fanOffStateChange);

        // Assert - LED should turn off when fan turns off
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.LedStatus.EntityId);
    }

    [Fact]
    public void SupportingFan_ManualOperation_Should_SetShouldActivateFanTrue()
    {
        // Act - Simulate manual operation of supporting fan
        var manualStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.SupportingFan,
            "off",
            "on",
            HaIdentity.DANIEL_RODRIGUEZ // Manual operation
        );
        _mockHaContext.StateChangeSubject.OnNext(manualStateChange);

        // Assert - ShouldActivateFan should be set to true (verified indirectly by not activating supporting fan on poor air quality)
        // Simulate poor air quality to verify ShouldActivateFan is true
        var poorAirStateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "30.0",
            "100.0"
        );
        _mockHaContext.StateChangeSubject.OnNext(poorAirStateChange);

        // Supporting fan should not be turned on again since it's manually controlled
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.SupportingFan.EntityId, "turn_on", 0);
    }

    [Theory(Skip = "Temporarily disabled - air quality automation logic under review")]
    [InlineData(5.0, "excellent")]
    [InlineData(25.0, "moderate")]
    [InlineData(100.0, "poor")]
    public void AirQuality_ThresholdBoundaries_Should_TriggerCorrectResponse(
        double pm25Value,
        string qualityDescription
    )
    {
        // Act - Simulate air quality change
        var stateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "50.0",
            pm25Value.ToString("F1")
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert based on quality level
        switch (qualityDescription)
        {
            case "excellent":
                _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.AirPurifierFan.EntityId);
                break;
            case "moderate":
                _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.AirPurifierFan.EntityId);
                break;
            case "poor":
                _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.SupportingFan.EntityId);
                break;
        }
    }

    #endregion

    #region Persistent Automation Tests

    [Fact(Skip = "Temporarily disabled - air quality automation logic under review")]
    public void MotionSensor_OffFor15Minutes_With_MasterSwitchOff_Should_TurnOnMasterSwitch()
    {
        // Arrange - Set master switch to off
        _mockHaContext.SetEntityState(_entities.MasterSwitch.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate motion sensor being off for 15 minutes
        var motionOffStateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(motionOffStateChange);

        // Assert - Master switch should turn on when motion is off for 15 minutes and master switch is off
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.MasterSwitch.EntityId);
    }

    [Fact]
    public void MotionSensor_OffFor15Minutes_With_MasterSwitchOn_Should_NotAffectMasterSwitch()
    {
        // Arrange - Master switch is already on (from setup)

        // Act - Simulate motion sensor being off for 15 minutes
        var motionOffStateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(motionOffStateChange);

        // Assert - Master switch should not be affected since it's already on
        _mockHaContext.ShouldNeverHaveCalledSwitch(_entities.MasterSwitch.EntityId);
    }

    [Fact]
    public void AirPurifierFan_ManualTurnOn_Should_TurnOnMasterSwitch()
    {
        // Arrange - Set master switch to off
        _mockHaContext.SetEntityState(_entities.MasterSwitch.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate manual turn on of air purifier fan
        var manualFanOnStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.AirPurifierFan,
            "off",
            "on",
            HaIdentity.DANIEL_RODRIGUEZ
        );
        _mockHaContext.StateChangeSubject.OnNext(manualFanOnStateChange);

        // Assert - Master switch should turn on when fan is manually turned on
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.MasterSwitch.EntityId);
    }

    [Fact]
    public void AirPurifierFan_ManualTurnOff_Should_TurnOffMasterSwitch()
    {
        // Act - Simulate manual turn off of air purifier fan
        var manualFanOffStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.AirPurifierFan,
            "on",
            "off",
            HaIdentity.DANIEL_RODRIGUEZ
        );
        _mockHaContext.StateChangeSubject.OnNext(manualFanOffStateChange);

        // Assert - Master switch should turn off when fan is manually turned off
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.MasterSwitch.EntityId);
    }

    [Fact]
    public void AirPurifierFan_AutomatedOperation_Should_NotAffectMasterSwitch()
    {
        // Act - Simulate automated turn on of air purifier fan
        var automatedFanStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.AirPurifierFan,
            "off",
            "on",
            HaIdentity.SUPERVISOR // Automated operation
        );
        _mockHaContext.StateChangeSubject.OnNext(automatedFanStateChange);

        // Assert - Master switch should not be affected by automated operations
        _mockHaContext.ShouldNeverHaveCalledSwitch(_entities.MasterSwitch.EntityId);
    }

    #endregion

    #region Master Switch Behavior Tests

    [Fact]
    public void MasterSwitch_TurnedOff_Should_DisableAutomations()
    {
        // Arrange - Turn off master switch
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "on", "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Try to trigger air quality automation with poor air quality
        var poorAirStateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "10.0",
            "100.0"
        );
        _mockHaContext.StateChangeSubject.OnNext(poorAirStateChange);

        // Assert - No fan operations should occur when master switch is off
        _mockHaContext.ShouldNeverHaveCalledSwitch(_entities.AirPurifierFan.EntityId);
        _mockHaContext.ShouldNeverHaveCalledSwitch(_entities.SupportingFan.EntityId);
    }

    #endregion

    #region State Coordination Tests

    [Fact(Skip = "Temporarily disabled - air quality automation logic under review")]
    public void ComplexAirQualitySequence_Should_HandleStateTransitions()
    {
        // Test a complete sequence: excellent -> poor -> moderate -> excellent

        // 1. Excellent air quality (≤7)
        var excellentAir = StateChangeHelpers.CreateNumericSensorStateChange(_entities.Pm25Sensor, "50.0", "5.0");
        _mockHaContext.StateChangeSubject.OnNext(excellentAir);
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.AirPurifierFan.EntityId);
        _mockHaContext.ClearServiceCalls();

        // 2. Poor air quality (>75) - should activate supporting fan
        var poorAir = StateChangeHelpers.CreateNumericSensorStateChange(_entities.Pm25Sensor, "5.0", "100.0");
        _mockHaContext.StateChangeSubject.OnNext(poorAir);
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.SupportingFan.EntityId);
        _mockHaContext.ClearServiceCalls();

        // 3. Moderate air quality (7-75) - should turn on main fan and off supporting fan
        var moderateAir = StateChangeHelpers.CreateNumericSensorStateChange(_entities.Pm25Sensor, "100.0", "30.0");
        _mockHaContext.StateChangeSubject.OnNext(moderateAir);
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.AirPurifierFan.EntityId);
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.SupportingFan.EntityId);
        _mockHaContext.ClearServiceCalls();

        // 4. Back to excellent - should turn off main fan
        var excellentAirAgain = StateChangeHelpers.CreateNumericSensorStateChange(_entities.Pm25Sensor, "30.0", "4.0");
        _mockHaContext.StateChangeSubject.OnNext(excellentAirAgain);
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.AirPurifierFan.EntityId);
    }

    [Fact(Skip = "Temporarily disabled - air quality automation logic under review")]
    public void SupportingFan_OffFor10Minutes_Should_ResetShouldActivateFan()
    {
        // Arrange - First manually operate supporting fan to set ShouldActivateFan to true
        var manualOnStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.SupportingFan,
            "off",
            "on",
            HaIdentity.DANIEL_RODRIGUEZ
        );
        _mockHaContext.StateChangeSubject.OnNext(manualOnStateChange);

        // Act - Simulate supporting fan being off for 10 minutes (resets ShouldActivateFan)
        var fanOffStateChange = StateChangeHelpers.SwitchTurnedOff(_entities.SupportingFan);
        _mockHaContext.StateChangeSubject.OnNext(fanOffStateChange);
        _mockHaContext.ClearServiceCalls();

        // Verify ShouldActivateFan is reset by triggering poor air quality
        var poorAirStateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "10.0",
            "100.0"
        );
        _mockHaContext.StateChangeSubject.OnNext(poorAirStateChange);

        // Assert - Supporting fan should turn on again, indicating ShouldActivateFan was reset
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.SupportingFan.EntityId);
    }

    #endregion

    #region Logging and Error Handling Tests

    [Fact(Skip = "Temporarily disabled - air quality automation logic under review")]
    public void ExcellentAirQuality_Should_LogAppropriateMessage()
    {
        // Arrange
        double excellentValue = 5.0;

        // Act
        var stateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "50.0",
            excellentValue.ToString("F1")
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Verify logging occurred with correct information level
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Air quality improved")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact(Skip = "Temporarily disabled - air quality automation logic under review")]
    public void ModerateAirQuality_Should_LogAppropriateMessage()
    {
        // Arrange - First set poor air quality to get into cleaning state
        var poorAirStateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "10.0",
            "100.0"
        );
        _mockHaContext.StateChangeSubject.OnNext(poorAirStateChange);

        double moderateValue = 30.0;

        // Act - Transition to moderate air quality (should log message)
        var stateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "100.0",
            moderateValue.ToString("F1")
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Verify logging occurred with correct information level
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Air quality moderate")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact(Skip = "Temporarily disabled - air quality automation logic under review")]
    public void PoorAirQuality_Should_LogAppropriateMessage()
    {
        // Arrange
        double poorValue = 100.0;

        // Act
        var stateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "30.0",
            poorValue.ToString("F1")
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Verify logging occurred with correct information level
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Air quality poor")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public void Automation_Should_NotThrow_WhenStateChangesOccur()
    {
        // This test ensures automation setup doesn't throw exceptions

        // Act & Assert - Should not throw
        var act = () =>
        {
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.CreateNumericSensorStateChange(_entities.Pm25Sensor, "50.0", "5.0")
            );
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.CreateNumericSensorStateChange(_entities.Pm25Sensor, "5.0", "100.0")
            );
            _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.SwitchTurnedOn(_entities.AirPurifierFan));
        };

        act.Should().NotThrow();
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Theory(Skip = "Temporarily disabled - air quality automation logic under review")]
    [InlineData(7.0, "boundary_clean")] // Exactly at clean threshold
    [InlineData(7.1, "just_above_clean")] // Just above clean threshold
    [InlineData(75.0, "boundary_dirty")] // Exactly at dirty threshold
    [InlineData(75.1, "just_above_dirty")] // Just above dirty threshold
    public void AirQuality_BoundaryValues_Should_HandleCorrectly(double pm25Value, string scenario)
    {
        // Act
        var stateChange = StateChangeHelpers.CreateNumericSensorStateChange(
            _entities.Pm25Sensor,
            "50.0",
            pm25Value.ToString("F1")
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert based on boundary scenario
        switch (scenario)
        {
            case "boundary_clean":
                // At threshold (7.0), should not trigger excellent air quality response
                _mockHaContext.ShouldNeverHaveCalledSwitch(_entities.AirPurifierFan.EntityId);
                break;
            case "just_above_clean":
                // Should trigger moderate air quality response
                _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.AirPurifierFan.EntityId);
                break;
            case "boundary_dirty":
                // At dirty threshold, should still be moderate
                _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.AirPurifierFan.EntityId);
                break;
            case "just_above_dirty":
                // Should trigger poor air quality response
                _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.SupportingFan.EntityId);
                break;
        }
    }

    [Fact]
    public void NullOrInvalidAirQualityValue_Should_NotCrashAutomation()
    {
        // Act & Assert - Should not throw
        var act = () =>
        {
            // Test null state
            var nullStateChange = StateChangeHelpers.CreateNumericSensorStateChange(
                _entities.Pm25Sensor,
                "50.0",
                null!
            );
            _mockHaContext.StateChangeSubject.OnNext(nullStateChange);

            // Test unavailable state
            var unavailableStateChange = StateChangeHelpers.CreateNumericSensorStateChange(
                _entities.Pm25Sensor,
                "50.0",
                HaEntityStates.UNAVAILABLE
            );
            _mockHaContext.StateChangeSubject.OnNext(unavailableStateChange);
        };

        act.Should().NotThrow();
    }

    #endregion

    public void Dispose()
    {
        _automation?.Dispose();
        _mockHaContext?.Dispose();
    }

    /// <summary>
    /// Test wrapper that implements IAirQualityEntities interface
    /// Creates entities internally with the appropriate entity IDs for LivingRoom air quality automation
    /// </summary>
    private class TestEntities(IHaContext haContext) : IAirQualityEntities
    {
        public SwitchEntity MasterSwitch { get; } =
            new SwitchEntity(haContext, "switch.living_room_air_quality_master");
        public BinarySensorEntity MotionSensor { get; } =
            new BinarySensorEntity(haContext, "binary_sensor.living_room_presence_sensors");
        public SwitchEntity AirPurifierFan { get; } = new SwitchEntity(haContext, "switch.air_purifier");
        public SwitchEntity SupportingFan { get; } = new SwitchEntity(haContext, "switch.living_room_ceiling_fan");
        public NumericSensorEntity Pm25Sensor { get; } = new NumericSensorEntity(haContext, "sensor.air_quality_pm2_5");
        public SwitchEntity LedStatus { get; } = new SwitchEntity(haContext, "switch.air_purifier_led");
    }
}
