using HomeAutomation.apps.Area.Bedroom.Config;
using HomeAutomation.apps.Area.Bedroom.Services.Schedulers.Entities;
using HomeAutomation.apps.Common.Settings;

namespace HomeAutomation.Tests.Common.Services;

public class ClimateSettingsResolverTests : HaContextTestBase
{
    private MockHaContext _mockHaContext => HaContext;
    private readonly Mock<
        ILogger<HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateSettingsResolver>
    > _mockLogger;
    private readonly Mock<HomeAutomation.apps.Area.Bedroom.Services.Schedulers.IAcTemperatureCalculator> _mockCalculator;
    private readonly Mock<IAreaSettingsStore> _mockAreaConfigStore;
    private readonly TestSchedulerEntities _schedulerEntities;
    private readonly HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateSettingsResolver _scheduler;

    public ClimateSettingsResolverTests()
    {
        _mockLogger =
            new Mock<
                ILogger<HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateSettingsResolver>
            >();
        _mockCalculator =
            new Mock<HomeAutomation.apps.Area.Bedroom.Services.Schedulers.IAcTemperatureCalculator>();
        _mockAreaConfigStore = new Mock<IAreaSettingsStore>();
        _schedulerEntities = new TestSchedulerEntities(_mockHaContext);
        _mockAreaConfigStore
            .Setup(x => x.GetSettings<ClimateSettings>("bedroom"))
            .Returns(CreateClimateSettings());

        _scheduler =
            new HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateSettingsResolver(
                _schedulerEntities,
                _mockAreaConfigStore.Object,
                _mockCalculator.Object,
                _mockLogger.Object
            );
    }

    [Theory]
    [InlineData(10, HomeAutomation.apps.Area.Bedroom.Config.TimeBlock.Sunrise, 24, "cool")]
    [InlineData(19, HomeAutomation.apps.Area.Bedroom.Config.TimeBlock.Sunset, 23, "cool")]
    [InlineData(2, HomeAutomation.apps.Area.Bedroom.Config.TimeBlock.Midnight, 22, "cool")]
    public void TryGetCurrentSetting_ShouldReturnExpectedBlockAndSetting(
        int hour,
        HomeAutomation.apps.Area.Bedroom.Config.TimeBlock expectedBlock,
        int expectedComfortTemp,
        string expectedMode
    )
    {
        SetSchedulerToLocalTime(hour);
        var scheduler = CreateScheduler();

        var success = scheduler.TryGetCurrentSetting(out var block, out var setting);

        success.Should().BeTrue();
        block.Should().Be(expectedBlock);
        setting.ComfortTemp.Should().Be(expectedComfortTemp);
        setting.Mode.Should().Be(expectedMode);
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
        SetSchedulerToLocalTime(19);

        _scheduler.TryGetCurrentSetting(out _, out var setting).Should().BeTrue();

        _mockCalculator
            .Setup(x =>
                x.CalculateTemperature(
                    setting,
                    isOccupied: true,
                    isDoorOpen: false,
                    expectedPowerSaving
                )
            )
            .Returns(26);

        var result = _scheduler.CalculateTemperature(setting, isOccupied: true, isDoorOpen: false);

        result.Should().Be(26);
    }

    [Fact]
    public void GetWeatherPowerSavingSettings_ShouldReturnConfiguredThresholds()
    {
        var settings = _scheduler.GetWeatherPowerSavingSettings();

        settings.TriggerUvIndex.Should().Be(8);
        settings.TriggerOutdoorTempC.Should().Be(32);
        settings.RecoveryUvIndex.Should().Be(5);
        settings.RecoveryOutdoorTempC.Should().Be(30);
    }

    [Fact]
    public void GetSchedules_WithInvalidHours_ShouldLogWarningAndSkipInvalidBlock()
    {
        var invalidStore = new Mock<IAreaSettingsStore>();
        invalidStore
            .Setup(x => x.GetSettings<ClimateSettings>("bedroom"))
            .Returns(CreateClimateSettings(sunriseHourStart: 27));
        var scheduler =
            new HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateSettingsResolver(
                _schedulerEntities,
                invalidStore.Object,
                _mockCalculator.Object,
                _mockLogger.Object
            );

        var schedules = scheduler.GetSchedules(() => { }).ToList();

        schedules.Should().HaveCount(2);
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Invalid HourStart")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public void TryGetCurrentSetting_WithNoMatchingRange_ShouldReturnFalse()
    {
        var store = new Mock<IAreaSettingsStore>();
        store
            .Setup(x => x.GetSettings<ClimateSettings>("bedroom"))
            .Returns(
                new ClimateSettings
                {
                    Sunrise = new ClimateSetting(25, 27, 24, 27, "cool", true, 27, 30),
                    Sunset = new ClimateSetting(25, 27, 23, 27, "cool", false, 27, 30),
                    Midnight = new ClimateSetting(24, 25, 22, 25, "cool", false, 27, 30),
                    WeatherPowerSaving = new WeatherPowerSavingSettings
                    {
                        TriggerUvIndex = 8,
                        TriggerOutdoorTempC = 32,
                        RecoveryUvIndex = 5,
                        RecoveryOutdoorTempC = 30,
                    },
                    Automation = new ClimateAutomationSettings(),
                    Light = new BedroomLightSettings(),
                }
            );
        var scheduler =
            new HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateSettingsResolver(
                _schedulerEntities,
                store.Object,
                _mockCalculator.Object,
                _mockLogger.Object
            );

        scheduler.TryGetCurrentSetting(out _, out _).Should().BeFalse();
    }

    private HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateSettingsResolver CreateScheduler() =>
        new HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateSettingsResolver(
            _schedulerEntities,
            _mockAreaConfigStore.Object,
            _mockCalculator.Object,
            _mockLogger.Object
        );

    private void SetSchedulerToLocalTime(int hour, int minute = 0)
    {
        var localTime = new DateTime(2024, 1, 1, hour, minute, 0, DateTimeKind.Unspecified);
        var offset = TimeZoneInfo.Local.GetUtcOffset(localTime);
        var schedulerTime = new DateTimeOffset(localTime, offset).ToUniversalTime();
        _mockHaContext.AdvanceTimeTo(schedulerTime);
    }

    private class TestSchedulerEntities(IHaContext haContext) : IClimateSchedulerEntities
    {
        public InputBooleanEntity PowerSavingMode { get; } =
            new InputBooleanEntity(haContext, "input_boolean.power_saving_mode");
    }

    private static ClimateSettings CreateClimateSettings(int sunriseHourStart = 5) =>
        new()
        {
            Sunrise = new ClimateSetting(25, 27, 24, 27, "cool", true, sunriseHourStart, 18),
            Sunset = new ClimateSetting(25, 27, 23, 27, "cool", false, 18, 0),
            Midnight = new ClimateSetting(24, 25, 22, 25, "cool", false, 0, 5),
            WeatherPowerSaving = new WeatherPowerSavingSettings
            {
                TriggerUvIndex = 8,
                TriggerOutdoorTempC = 32,
                RecoveryUvIndex = 5,
                RecoveryOutdoorTempC = 30,
            },
            Automation = new ClimateAutomationSettings(),
            Light = new BedroomLightSettings(),
        };
}
