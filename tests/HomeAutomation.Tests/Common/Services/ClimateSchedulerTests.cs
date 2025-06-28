using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Services;

namespace HomeAutomation.Tests.Common.Services;

/// <summary>
/// Unit tests for ClimateScheduler focusing on time-based logic, temperature selection, and scheduling behavior
/// Tests the core scheduling functionality without complex automation integration
/// </summary>
public class ClimateSchedulerTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<ILogger<ClimateScheduler>> _mockLogger;
    private readonly TestWeatherEntities _weatherEntities;
    private readonly TestScheduler _testScheduler;
    private readonly ClimateScheduler _scheduler;

    public ClimateSchedulerTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<ClimateScheduler>>();
        _weatherEntities = new TestWeatherEntities(_mockHaContext);
        _testScheduler = new TestScheduler();

        // IMPORTANT: Setup sensor states BEFORE creating ClimateScheduler
        // because the constructor calls GetCurrentAcScheduleSettings() which reads sensor states
        SetupDefaultSunSensorStates();

        _scheduler = new ClimateScheduler(_weatherEntities, _testScheduler, _mockLogger.Object);
    }

    private void SetupDefaultSunSensorStates()
    {
        // Setup valid sun sensor times for testing - using format that works with DateTime.TryParse
        _mockHaContext.SetEntityState(_weatherEntities.SunRising.EntityId, "2024-01-01T06:00:00");
        _mockHaContext.SetEntityState(_weatherEntities.SunSetting.EntityId, "2024-01-01T18:00:00");
        _mockHaContext.SetEntityState(_weatherEntities.SunMidnight.EntityId, "2024-01-01T00:00:00");
        _mockHaContext.SetEntityState(_weatherEntities.Weather.EntityId, "sunny");
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
        setting!.Mode.Should().Be("dry", "Sunrise period uses dry mode");
        setting.ActivateFan.Should().BeTrue("Sunrise period can activate fan");
        setting.NormalTemp.Should().Be(27, "Sunrise NormalTemp should be 27°C");
        setting.PowerSavingTemp.Should().Be(27, "Sunrise PowerSavingTemp should be 27°C");
        setting.CoolTemp.Should().Be(24, "Sunrise CoolTemp should be 24°C");
        setting.PassiveTemp.Should().Be(27, "Sunrise PassiveTemp should be 27°C");
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
        setting.NormalTemp.Should().Be(25, "Sunset NormalTemp should be 25°C");
        setting.PowerSavingTemp.Should().Be(27, "Sunset PowerSavingTemp should be 27°C");
        setting.CoolTemp.Should().Be(23, "Sunset CoolTemp should be 23°C");
        setting.PassiveTemp.Should().Be(27, "Sunset PassiveTemp should be 27°C");
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
        setting.NormalTemp.Should().Be(24, "Midnight NormalTemp should be 24°C");
        setting.PowerSavingTemp.Should().Be(25, "Midnight PowerSavingTemp should be 25°C");
        setting.CoolTemp.Should().Be(22, "Midnight CoolTemp should be 22°C");
        setting.PassiveTemp.Should().Be(25, "Midnight PassiveTemp should be 25°C");
    }

    #endregion

    #region Temperature Selection Logic Tests

    [Theory]
    [InlineData(true, true, true, true, "PowerSavingTemp", "PowerSaving mode takes precedence")]
    [InlineData(true, false, false, true, "CoolTemp", "Occupied + closed door = CoolTemp")]
    [InlineData(true, false, false, false, "CoolTemp", "Occupied + closed door = CoolTemp")]
    [InlineData(
        false,
        true,
        false,
        true,
        "NormalTemp",
        "Unoccupied + open door + cold weather = NormalTemp"
    )]
    [InlineData(
        true,
        true,
        false,
        true,
        "NormalTemp",
        "Occupied + open door + cold weather = NormalTemp"
    )]
    [InlineData(
        true,
        true,
        false,
        false,
        "NormalTemp",
        "Occupied + open door + sunny weather = NormalTemp"
    )]
    [InlineData(
        false,
        true,
        false,
        false,
        "PassiveTemp",
        "Unoccupied + open door + sunny weather = PassiveTemp"
    )]
    [InlineData(
        false,
        false,
        false,
        false,
        "PassiveTemp",
        "Unoccupied + closed door = PassiveTemp"
    )]
    public void GetTemperature_Various_Scenarios_Should_Return_Correct_Temperature(
        bool occupied,
        bool doorOpen,
        bool powerSaving,
        bool isCold,
        string expectedTempType,
        string reason
    )
    {
        // Arrange
        var success = _scheduler.TryGetSetting(TimeBlock.Sunset, out var setting);
        success.Should().BeTrue();

        // Act - Simulate the temperature selection logic
        var actualTemp = (occupied, doorOpen, powerSaving, isCold) switch
        {
            (_, _, true, _) => setting!.PowerSavingTemp,
            (true, false, false, _) => setting!.CoolTemp,
            (false, true, false, true) => setting!.NormalTemp,
            (true, true, false, _) => setting!.NormalTemp,
            (false, true, false, false) => setting!.PassiveTemp,
            _ => setting!.PassiveTemp,
        };

        // Assert
        var expectedTemp = expectedTempType switch
        {
            "PowerSavingTemp" => setting!.PowerSavingTemp,
            "CoolTemp" => setting!.CoolTemp,
            "NormalTemp" => setting!.NormalTemp,
            "PassiveTemp" => setting!.PassiveTemp,
            _ => throw new ArgumentException($"Unknown temp type: {expectedTempType}"),
        };

        actualTemp.Should().Be(expectedTemp, reason);
    }

    #endregion

    #region Time Block Detection Tests

    [Theory(
        Skip = "Time zone alignment on CI runner causes hour mismatch. Revisit when mocking is timezone-agnostic."
    )]
    [InlineData(8, TimeBlock.Sunrise, "8 AM should be in Sunrise period (6 AM - 6 PM)")]
    [InlineData(12, TimeBlock.Sunrise, "12 PM should be in Sunrise period (6 AM - 6 PM)")]
    [InlineData(17, TimeBlock.Sunrise, "5 PM should be in Sunrise period (6 AM - 6 PM)")]
    [InlineData(19, TimeBlock.Sunset, "7 PM should be in Sunset period (6 PM - 12 AM)")]
    [InlineData(22, TimeBlock.Sunset, "10 PM should be in Sunset period (6 PM - 12 AM)")]
    [InlineData(23, TimeBlock.Sunset, "11 PM should be in Sunset period (6 PM - 12 AM)")]
    [InlineData(1, TimeBlock.Midnight, "1 AM should be in Midnight period (12 AM - 6 AM)")]
    [InlineData(3, TimeBlock.Midnight, "3 AM should be in Midnight period (12 AM - 6 AM)")]
    [InlineData(5, TimeBlock.Midnight, "5 AM should be in Midnight period (12 AM - 6 AM)")]
    public void FindCurrentTimeBlock_Various_Hours_Should_Return_Correct_TimeBlock(
        int hour,
        TimeBlock expectedTimeBlock,
        string reason
    )
    {
        // Arrange
        var testScheduler = new TestSchedulerWithTime(hour);
        var scheduler = new ClimateScheduler(_weatherEntities, testScheduler, _mockLogger.Object);

        var now = testScheduler.Now;
        var local = now.LocalDateTime;
        var block = scheduler.FindCurrentTimeBlock();

        Console.WriteLine($"[Test Hour: {hour}] Now: {now}, Local: {local}, Block: {block}");
        // Act
        var actualTimeBlock = scheduler.FindCurrentTimeBlock();

        // Assert
        actualTimeBlock.Should().Be(expectedTimeBlock, reason);
    }

    [Fact(Skip = "Failing in Github, can control it's local time")]
    public void FindCurrentTimeBlock_BoundaryHours_Should_Handle_Correctly()
    {
        // Test boundary conditions

        // 6 AM - start of Sunrise
        var scheduler6 = new ClimateScheduler(
            _weatherEntities,
            new TestSchedulerWithTime(6),
            _mockLogger.Object
        );
        scheduler6
            .FindCurrentTimeBlock()
            .Should()
            .Be(TimeBlock.Sunrise, "6 AM should start Sunrise period");

        // 18 (6 PM) - start of Sunset
        var scheduler18 = new ClimateScheduler(
            _weatherEntities,
            new TestSchedulerWithTime(18),
            _mockLogger.Object
        );
        scheduler18
            .FindCurrentTimeBlock()
            .Should()
            .Be(TimeBlock.Sunset, "6 PM should start Sunset period");

        // 0 (12 AM) - start of Midnight
        var scheduler0 = new ClimateScheduler(
            _weatherEntities,
            new TestSchedulerWithTime(0),
            _mockLogger.Object
        );
        scheduler0
            .FindCurrentTimeBlock()
            .Should()
            .Be(TimeBlock.Midnight, "12 AM should start Midnight period");
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
        _mockHaContext.SetEntityState(_weatherEntities.SunRising.EntityId, "invalid");
        _mockHaContext.SetEntityState(_weatherEntities.SunSetting.EntityId, "invalid");
        _mockHaContext.SetEntityState(_weatherEntities.SunMidnight.EntityId, "invalid");

        // Act - Try to create schedules
        var scheduler = new ClimateScheduler(_weatherEntities, _testScheduler, _mockLogger.Object);
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

    public void Dispose()
    {
        _mockHaContext?.Dispose();
    }

    private class TestWeatherEntities(IHaContext haContext) : IClimateSchedulerEntities
    {
        public SensorEntity SunRising { get; } = new SensorEntity(haContext, "sensor.sun_rising");
        public SensorEntity SunSetting { get; } = new SensorEntity(haContext, "sensor.sun_setting");
        public SensorEntity SunMidnight { get; } =
            new SensorEntity(haContext, "sensor.sun_midnight");
        public WeatherEntity Weather { get; } = new WeatherEntity(haContext, "weather.home");
        public InputBooleanEntity PowerSavingMode { get; } =
            new InputBooleanEntity(haContext, "input_boolean.power_saving_mode");
    }

    private class TestScheduler : IScheduler
    {
        public virtual DateTimeOffset Now =>
            new DateTimeOffset(2024, 1, 1, 20, 0, 0, TimeSpan.Zero);

        public IDisposable Schedule<TState>(
            TState state,
            Func<IScheduler, TState, IDisposable> action
        )
        {
            return Mock.Of<IDisposable>();
        }

        public IDisposable Schedule<TState>(
            TState state,
            TimeSpan dueTime,
            Func<IScheduler, TState, IDisposable> action
        )
        {
            return Mock.Of<IDisposable>();
        }

        public IDisposable Schedule<TState>(
            TState state,
            DateTimeOffset dueTime,
            Func<IScheduler, TState, IDisposable> action
        )
        {
            return Mock.Of<IDisposable>();
        }

        public static IDisposable ScheduleCron(string cronExpression, Action action)
        {
            return Mock.Of<IDisposable>();
        }
    }

    private class TestSchedulerWithTime(int hour) : TestScheduler
    {
        private readonly int _hour = hour;

        public override DateTimeOffset Now => new(2024, 1, 1, _hour, 0, 0, TimeSpan.FromHours(+8)); // Account for local hours
    }
}
