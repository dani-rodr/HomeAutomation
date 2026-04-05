using HomeAutomation.apps.Area.Bedroom.Config;
using HomeAutomation.apps.Area.Bedroom.Services.Schedulers.Entities;
using HomeAutomation.apps.Common.Settings;

namespace HomeAutomation.Tests.Common.Services;

public class ClimateAutomationSchedulerTests : HaContextTestBase
{
    private MockHaContext _mockHaContext => HaContext;
    private readonly Mock<
        ILogger<HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateAutomationScheduler>
    > _mockLogger;
    private readonly Mock<HomeAutomation.apps.Area.Bedroom.Services.Schedulers.IAcTemperatureCalculator> _mockCalculator;
    private readonly Mock<ILiveAppConfig<BedroomSettings>> _mockLiveSettings;
    private readonly TestSchedulerEntities _schedulerEntities;
    private readonly HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateAutomationScheduler _climateAutomationScheduler;

    public ClimateAutomationSchedulerTests()
    {
        _mockLogger =
            new Mock<
                ILogger<HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateAutomationScheduler>
            >();
        _mockCalculator =
            new Mock<HomeAutomation.apps.Area.Bedroom.Services.Schedulers.IAcTemperatureCalculator>();
        _mockLiveSettings = new Mock<ILiveAppConfig<BedroomSettings>>();
        _schedulerEntities = new TestSchedulerEntities(_mockHaContext);
        var settings = CreateBedroomSettings();
        _mockLiveSettings.SetupGet(x => x.Value).Returns(settings);
        _mockLiveSettings.SetupGet(x => x.Settings).Returns(settings);
        _mockLiveSettings.SetupGet(x => x.Changes).Returns(Observable.Empty<BedroomSettings>());

        _climateAutomationScheduler =
            new HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateAutomationScheduler(
                _schedulerEntities,
                _mockLiveSettings.Object,
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
        var climateAutomationScheduler = CreateClimateAutomationScheduler();

        var success = climateAutomationScheduler.TryGetCurrentSetting(
            out var block,
            out var setting
        );

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

        _climateAutomationScheduler.TryGetCurrentSetting(out _, out var setting).Should().BeTrue();

        _mockCalculator
            .Setup(x =>
                x.CalculateTemperature(
                    setting,
                    isOccupied: true,
                    isDoorOpen: false,
                    expectedPowerSaving,
                    2
                )
            )
            .Returns(26);

        var result = _climateAutomationScheduler.CalculateTemperature(
            setting,
            isOccupied: true,
            isDoorOpen: false
        );

        result.Should().Be(26);
    }

    [Fact]
    public void GetCurrentSettings_ShouldReturnConfiguredThresholds()
    {
        var settings = _climateAutomationScheduler.GetCurrentSettings().WeatherPowerSaving;

        settings.TriggerUvIndex.Should().Be(8);
        settings.TriggerOutdoorTempC.Should().Be(32);
        settings.RecoveryUvIndex.Should().Be(5);
        settings.RecoveryOutdoorTempC.Should().Be(30);
    }

    [Fact]
    public void GetSchedules_WithInvalidHours_ShouldLogWarningAndSkipInvalidBlock()
    {
        var invalidSettings = CreateBedroomSettings(sunriseHourStart: 27);
        var invalidLiveSettings = new Mock<ILiveAppConfig<BedroomSettings>>();
        invalidLiveSettings.SetupGet(x => x.Value).Returns(invalidSettings);
        invalidLiveSettings.SetupGet(x => x.Settings).Returns(invalidSettings);
        invalidLiveSettings.SetupGet(x => x.Changes).Returns(Observable.Empty<BedroomSettings>());
        var climateAutomationScheduler =
            new HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateAutomationScheduler(
                _schedulerEntities,
                invalidLiveSettings.Object,
                _mockCalculator.Object,
                _mockLogger.Object
            );

        var schedules = climateAutomationScheduler.GetSchedules(() => { }).ToList();

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
        var settings = new BedroomSettings
        {
            Climate = new ClimateSettings
            {
                Sunrise = new ClimateSetting(25, 24, 25, "cool", 27, 30),
                Sunset = new ClimateSetting(24, 23, 25, "cool", 27, 30),
                Midnight = new ClimateSetting(24, 22, 25, "cool", 27, 30),
                PowerSavingTempOffsetC = 2,
                EnableFanAssist = true,
                FanAssistAtOrAboveSetpointC = 25,
                WeatherPowerSaving = new WeatherPowerSavingSettings
                {
                    TriggerUvIndex = 8,
                    TriggerOutdoorTempC = 32,
                    RecoveryUvIndex = 5,
                    RecoveryOutdoorTempC = 30,
                },
                Automation = new ClimateAutomationSettings(),
            },
            Light = new BedroomLightSettings(),
        };
        var liveSettings = new Mock<ILiveAppConfig<BedroomSettings>>();
        liveSettings.SetupGet(x => x.Value).Returns(settings);
        liveSettings.SetupGet(x => x.Settings).Returns(settings);
        liveSettings.SetupGet(x => x.Changes).Returns(Observable.Empty<BedroomSettings>());
        var climateAutomationScheduler =
            new HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateAutomationScheduler(
                _schedulerEntities,
                liveSettings.Object,
                _mockCalculator.Object,
                _mockLogger.Object
            );

        climateAutomationScheduler.TryGetCurrentSetting(out _, out _).Should().BeFalse();
    }

    private HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateAutomationScheduler CreateClimateAutomationScheduler() =>
        new HomeAutomation.apps.Area.Bedroom.Services.Schedulers.ClimateAutomationScheduler(
            _schedulerEntities,
            _mockLiveSettings.Object,
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

    private static BedroomSettings CreateBedroomSettings(int sunriseHourStart = 5) =>
        new()
        {
            Climate = new ClimateSettings
            {
                Sunrise = new ClimateSetting(25, 24, 25, "cool", sunriseHourStart, 18),
                Sunset = new ClimateSetting(24, 23, 25, "cool", 18, 0),
                Midnight = new ClimateSetting(24, 22, 25, "cool", 0, 5),
                PowerSavingTempOffsetC = 2,
                EnableFanAssist = true,
                FanAssistAtOrAboveSetpointC = 25,
                WeatherPowerSaving = new WeatherPowerSavingSettings
                {
                    TriggerUvIndex = 8,
                    TriggerOutdoorTempC = 32,
                    RecoveryUvIndex = 5,
                    RecoveryOutdoorTempC = 30,
                },
                Automation = new ClimateAutomationSettings(),
            },
            Light = new BedroomLightSettings(),
        };
}
