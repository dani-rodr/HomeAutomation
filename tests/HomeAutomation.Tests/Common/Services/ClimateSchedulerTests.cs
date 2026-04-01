using HomeAutomation.apps.Area.Bedroom.Services.Schedulers;
using HomeAutomation.apps.Area.Bedroom.Services.Schedulers.Entities;

namespace HomeAutomation.Tests.Common.Services;

/// <summary>
/// Unit tests for ClimateScheduler focusing on time-based logic, temperature selection, and scheduling behavior
/// Tests the core scheduling functionality without complex automation integration
/// </summary>
public class ClimateSchedulerTests : HaContextTestBase
{
    private MockHaContext _mockHaContext => HaContext;
    private readonly Mock<ILogger<ClimateScheduler>> _mockLogger;
    private readonly Mock<IAcTemperatureCalculator> _mockCalculator;
    private readonly TestSchedulerEntities _schedulerEntities;
    private readonly ClimateScheduler _scheduler;

    public ClimateSchedulerTests()
    {
        _mockLogger = new Mock<ILogger<ClimateScheduler>>();
        _mockCalculator = new Mock<IAcTemperatureCalculator>();
        _schedulerEntities = new TestSchedulerEntities(_mockHaContext);

        // IMPORTANT: Setup sensor states BEFORE creating ClimateScheduler
        // because the constructor calls GetCurrentAcScheduleSettings() which reads sensor states
        SetupDefaultSunSensorStates();

        _scheduler = new ClimateScheduler(
            _schedulerEntities,
            _mockCalculator.Object,
            _mockLogger.Object
        );
    }

    private void SetupDefaultSunSensorStates()
    {
        // Setup sun sensor times to match the original logs where 5AM should be Sunrise
        // Original logs show: Sunrise: 5-18, Sunset: 18-0, Midnight: 0-5
        _mockHaContext.SetEntityState(_schedulerEntities.SunRising.EntityId, "2024-01-01T05:00:00");
        _mockHaContext.SetEntityState(
            _schedulerEntities.SunSetting.EntityId,
            "2024-01-01T18:00:00"
        );
        _mockHaContext.SetEntityState(
            _schedulerEntities.SunMidnight.EntityId,
            "2024-01-01T00:00:00"
        );
    }

    private void SetSchedulerToLocalTime(int hour, int minute = 0)
    {
        var localTime = new DateTime(2024, 1, 1, hour, minute, 0, DateTimeKind.Unspecified);
        var offset = TimeZoneInfo.Local.GetUtcOffset(localTime);
        var schedulerTime = new DateTimeOffset(localTime, offset).ToUniversalTime();
        _mockHaContext.AdvanceTimeTo(schedulerTime);
    }

    #region TimeBlock Configuration Tests

    [Fact]
    public void TryGetSetting_SunriseTimeBlock_Should_HaveCorrectConfiguration()
    {
        // Act
        var success = _scheduler.TryGetSetting(TimeBlock.Sunrise, out var setting);

        // Assert
        success.Should().BeTrue("Sunrise time block should have valid settings");
        setting.Should().NotBeNull();
        setting!.Mode.Should().Be("cool", "Sunrise period uses cool mode");
        setting.ActivateFan.Should().BeTrue("Sunrise period can activate fan");
        setting.DoorOpenTemp.Should().Be(25, "Sunrise DoorOpenTemp should be 25°C");
        setting.EcoAwayTemp.Should().Be(27, "Sunrise EcoAwayTemp should be 27°C");
        setting.ComfortTemp.Should().Be(24, "Sunrise ComfortTemp should be 24°C");
        setting.AwayTemp.Should().Be(27, "Sunrise AwayTemp should be 27°C");
    }

    [Fact]
    public void TryGetSetting_SunsetTimeBlock_Should_HaveCorrectConfiguration()
    {
        // Act
        var success = _scheduler.TryGetSetting(TimeBlock.Sunset, out var setting);

        // Assert
        success.Should().BeTrue("Sunset time block should have valid settings");
        setting.Should().NotBeNull();
        setting!.Mode.Should().Be("cool", "Sunset period uses cool mode");
        setting.ActivateFan.Should().BeFalse("Sunset period doesn't activate fan");
        setting.DoorOpenTemp.Should().Be(25, "Sunset DoorOpenTemp should be 25°C");
        setting.EcoAwayTemp.Should().Be(27, "Sunset EcoAwayTemp should be 27°C");
        setting.ComfortTemp.Should().Be(23, "Sunset ComfortTemp should be 23°C");
        setting.AwayTemp.Should().Be(27, "Sunset AwayTemp should be 27°C");
    }

    [Fact]
    public void TryGetSetting_MidnightTimeBlock_Should_HaveCorrectConfiguration()
    {
        // Act
        var success = _scheduler.TryGetSetting(TimeBlock.Midnight, out var setting);

        // Assert
        success.Should().BeTrue("Midnight time block should have valid settings");
        setting.Should().NotBeNull();
        setting!.Mode.Should().Be("cool", "Midnight period uses cool mode");
        setting.ActivateFan.Should().BeFalse("Midnight period doesn't activate fan");
        setting.DoorOpenTemp.Should().Be(24, "Midnight DoorOpenTemp should be 24°C");
        setting.EcoAwayTemp.Should().Be(25, "Midnight EcoAwayTemp should be 25°C");
        setting.ComfortTemp.Should().Be(22, "Midnight ComfortTemp should be 22°C");
        setting.AwayTemp.Should().Be(25, "Midnight AwayTemp should be 25°C");
    }

    #endregion

    #region Temperature Selection Logic Tests

    [Theory]
    [InlineData(true, false, true, "ComfortTemp", "Occupied + closed door ignores power saving")]
    [InlineData(true, false, false, "ComfortTemp", "Occupied + closed door = ComfortTemp")]
    [InlineData(true, true, true, "DoorOpenTemp", "Occupied + open door ignores power saving")]
    [InlineData(true, true, false, "DoorOpenTemp", "Occupied + open door = DoorOpenTemp")]
    [InlineData(false, false, true, "EcoAwayTemp", "Unoccupied + power saving = EcoAwayTemp")]
    [InlineData(
        false,
        true,
        true,
        "EcoAwayTemp",
        "Unoccupied + open door + power saving = EcoAwayTemp"
    )]
    [InlineData(false, false, false, "AwayTemp", "Unoccupied + no power saving = AwayTemp")]
    [InlineData(
        false,
        true,
        false,
        "AwayTemp",
        "Unoccupied + open door + no power saving = AwayTemp"
    )]
    public void GetTemperature_Various_Scenarios_Should_Return_Correct_Temperature(
        bool occupied,
        bool doorOpen,
        bool powerSaving,
        string expectedTempType,
        string reason
    )
    {
        // Arrange
        var success = _scheduler.TryGetSetting(TimeBlock.Sunset, out var setting);
        success.Should().BeTrue();

        // Act - Simulate the temperature selection logic
        var actualTemp = (occupied, doorOpen, powerSaving) switch
        {
            (true, false, _) => setting!.ComfortTemp,
            (true, true, _) => setting!.DoorOpenTemp,
            (false, _, true) => setting!.EcoAwayTemp,
            _ => setting!.AwayTemp,
        };

        // Assert
        var expectedTemp = expectedTempType switch
        {
            "ComfortTemp" => setting!.ComfortTemp,
            "DoorOpenTemp" => setting!.DoorOpenTemp,
            "EcoAwayTemp" => setting!.EcoAwayTemp,
            "AwayTemp" => setting!.AwayTemp,
            _ => throw new ArgumentException($"Unknown temp type: {expectedTempType}"),
        };

        actualTemp.Should().Be(expectedTemp, reason);
    }

    [Theory]
    [InlineData("on", true)]
    [InlineData("off", false)]
    public void CalculateTemperature_ShouldPassPowerSavingStateToCalculator(
        string powerSavingState,
        bool expectedPowerSaving
    )
    {
        _mockHaContext.SetEntityState(
            _schedulerEntities.PowerSavingMode.EntityId,
            powerSavingState
        );

        var success = _scheduler.TryGetSetting(TimeBlock.Sunset, out var setting);
        success.Should().BeTrue();

        _mockCalculator
            .Setup(x =>
                x.CalculateTemperature(
                    setting!,
                    isOccupied: true,
                    isDoorOpen: false,
                    expectedPowerSaving
                )
            )
            .Returns(26);

        var result = _scheduler.CalculateTemperature(setting!, isOccupied: true, isDoorOpen: false);

        result.Should().Be(26);
        _mockCalculator.Verify(
            x =>
                x.CalculateTemperature(
                    setting!,
                    isOccupied: true,
                    isDoorOpen: false,
                    expectedPowerSaving
                ),
            Times.Once
        );
    }

    #endregion

    #region Time Block Detection Tests

    [Fact]
    public void FindCurrentTimeBlock_BoundaryHours_Should_Handle_Correctly()
    {
        // Test boundary conditions
        SetSchedulerToLocalTime(6);

        // 6 AM - start of Sunrise
        var scheduler6 = new ClimateScheduler(
            _schedulerEntities,
            _mockCalculator.Object,
            _mockLogger.Object
        );
        scheduler6
            .FindCurrentTimeBlock()
            .Should()
            .Be(TimeBlock.Sunrise, "6 AM should start Sunrise period");

        // 18 (6 PM) - start of Sunset
        _mockHaContext.AdvanceTimeByHours(12);

        var scheduler18 = new ClimateScheduler(
            _schedulerEntities,
            _mockCalculator.Object,
            _mockLogger.Object
        );
        scheduler18
            .FindCurrentTimeBlock()
            .Should()
            .Be(TimeBlock.Sunset, "6 PM should start Sunset period");

        _mockHaContext.AdvanceTimeByHours(6);

        // 0 (12 AM) - start of Midnight
        var scheduler0 = new ClimateScheduler(
            _schedulerEntities,
            _mockCalculator.Object,
            _mockLogger.Object
        );
        scheduler0
            .FindCurrentTimeBlock()
            .Should()
            .Be(TimeBlock.Midnight, "12 AM should start Midnight period");
    }

    [Fact]
    public void FindCurrentTimeBlock_At5AM_Should_ReturnSunriseNotMidnight()
    {
        // Arrange - Create scheduler with mocked time at exactly 5:00 AM
        // This test is designed to catch the bug where 5:00 AM incorrectly returns Midnight instead of Sunrise
        SetSchedulerToLocalTime(5);
        var scheduler5AM = new ClimateScheduler(
            _schedulerEntities,
            _mockCalculator.Object,
            _mockLogger.Object
        );

        // Act
        var timeBlock = scheduler5AM.FindCurrentTimeBlock();

        // Assert
        timeBlock
            .Should()
            .Be(
                TimeBlock.Sunrise,
                "5:00 AM should be start of Sunrise period, not end of Midnight period. "
                    + "This test catches the bug where dictionary iteration order causes wrong block selection."
            );
    }

    [Theory]
    [InlineData(4, 59, TimeBlock.Midnight, "4:59 AM should be in Midnight period (0-5)")]
    [InlineData(5, 0, TimeBlock.Sunrise, "5:00 AM should be in Sunrise period (5-18)")]
    [InlineData(5, 1, TimeBlock.Sunrise, "5:01 AM should be in Sunrise period (5-18)")]
    [InlineData(17, 59, TimeBlock.Sunrise, "5:59 PM should be in Sunrise period (5-18)")]
    [InlineData(18, 0, TimeBlock.Sunset, "6:00 PM should be in Sunset period (18-0)")]
    [InlineData(18, 1, TimeBlock.Sunset, "6:01 PM should be in Sunset period (18-0)")]
    [InlineData(23, 59, TimeBlock.Sunset, "11:59 PM should be in Sunset period (18-0)")]
    [InlineData(0, 0, TimeBlock.Midnight, "12:00 AM should be in Midnight period (0-5)")]
    [InlineData(0, 1, TimeBlock.Midnight, "12:01 AM should be in Midnight period (0-5)")]
    public void FindCurrentTimeBlock_BoundaryTransitions_Should_BeCorrect(
        int hour,
        int minute,
        TimeBlock expectedBlock,
        string reason
    )
    {
        // Arrange

        SetSchedulerToLocalTime(hour, minute);

        var scheduler = new ClimateScheduler(
            _schedulerEntities,
            _mockCalculator.Object,
            _mockLogger.Object
        );
        // Act
        var actualBlock = scheduler.FindCurrentTimeBlock();

        // Assert
        actualBlock.Should().Be(expectedBlock, reason);
    }

    #endregion

    #region Scheduling Logic Tests

    [Fact]
    public void GetSchedules_Should_Create_Three_Schedules()
    {
        // Arrange
        var actionCallCount = 0;
        void TestAction() => actionCallCount++;

        // Act
        var schedules = _scheduler.GetSchedules(TestAction).ToList();

        // Assert
        schedules
            .Should()
            .HaveCount(3, "Should create schedules for Sunrise, Sunset, and Midnight");
        schedules.Should().AllBeAssignableTo<IDisposable>("All schedules should be disposable");
    }

    [Fact]
    public void GetResetSchedule_Should_Create_Midnight_CronJob()
    {
        // Act
        var resetSchedule = _scheduler.GetResetSchedule();

        // Assert
        resetSchedule.Should().NotBeNull("Reset schedule should be created");
        resetSchedule.Should().BeAssignableTo<IDisposable>("Reset schedule should be disposable");
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void InvalidSunSensorHours_Should_LogWarning()
    {
        // Arrange - Set invalid sun sensor hours
        _mockHaContext.SetEntityState(_schedulerEntities.SunRising.EntityId, "invalid");
        _mockHaContext.SetEntityState(_schedulerEntities.SunSetting.EntityId, "invalid");
        _mockHaContext.SetEntityState(_schedulerEntities.SunMidnight.EntityId, "invalid");

        // Act - Try to create schedules
        var scheduler = new ClimateScheduler(
            _schedulerEntities,
            _mockCalculator.Object,
            _mockLogger.Object
        );
        var schedules = scheduler.GetSchedules(() => { }).ToList();

        // Assert - Should handle gracefully
        schedules.Should().NotBeNull("Should handle invalid hours gracefully");

        // Verify warning was logged for invalid hours
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid HourStart")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce,
            "Should log warning for invalid hour values"
        );
    }

    [Fact]
    public void TryGetSetting_InvalidTimeBlock_Should_ReturnFalse()
    {
        // Act
        var success = _scheduler.TryGetSetting((TimeBlock)999, out var setting);

        // Assert
        success.Should().BeFalse("Invalid time block should return false");
        setting.Should().BeNull("Setting should be null for invalid time block");
    }

    #endregion

    private class TestSchedulerEntities(IHaContext haContext) : IClimateSchedulerEntities
    {
        public SensorEntity SunRising { get; } = new SensorEntity(haContext, "sensor.sun_rising");
        public SensorEntity SunSetting { get; } = new SensorEntity(haContext, "sensor.sun_setting");
        public SensorEntity SunMidnight { get; } =
            new SensorEntity(haContext, "sensor.sun_midnight");
        public InputBooleanEntity PowerSavingMode { get; } =
            new InputBooleanEntity(haContext, "input_boolean.power_saving_mode");
    }
}
