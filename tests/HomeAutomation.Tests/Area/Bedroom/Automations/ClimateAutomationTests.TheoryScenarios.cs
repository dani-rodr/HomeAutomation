using HomeAutomation.apps.Common.Services.Schedulers;

namespace HomeAutomation.Tests.Area.Bedroom.Automations;

public partial class ClimateAutomationTests
{
    #region Comprehensive Theory Tests for Temperature Selection

    [Theory]
    [InlineData(true, false, TimeBlock.Sunset, 23, "cool", "Occupied + closed door = CoolTemp")]
    [InlineData(false, true, TimeBlock.Sunset, 27, "cool", "Unoccupied + open door = PassiveTemp")]
    [InlineData(true, true, TimeBlock.Sunset, 25, "cool", "Occupied + open door = NormalTemp")]
    [InlineData(
        false,
        false,
        TimeBlock.Sunset,
        27,
        "cool",
        "Unoccupied + closed door = PassiveTemp"
    )]
    public void ClimateAutomation_TemperatureSelection_Should_Follow_Logic(
        bool occupied,
        bool doorOpen,
        TimeBlock timeBlock,
        int expectedTemp,
        string expectedMode,
        string scenario
    )
    {
        var testSetting = new AcSettings(
            NormalTemp: 25,
            PowerSavingTemp: 27,
            CoolTemp: 23,
            PassiveTemp: 27,
            Mode: expectedMode,
            ActivateFan: false,
            HourStart: 18,
            HourEnd: 0
        );

        SetupSchedulerMock(timeBlock, testSetting);
        _mockHaContext.ClearServiceCalls();

        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, occupied ? "on" : "off");
        _mockHaContext.SetEntityState(_entities.Door.EntityId, doorOpen ? "on" : "off");
        _mockHaContext.SetEntityState(_entities.AirConditioner.EntityId, "cool");
        _mockHaContext.ClearServiceCalls();

        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        _mockHaContext.ShouldHaveCalledClimateSetTemperature(
            _entities.AirConditioner.EntityId,
            expectedMode,
            expectedTemp
        );

        scenario.Should().NotBeEmpty("Test scenario should be documented");
    }

    [Theory]
    [InlineData(
        TimeBlock.Sunrise,
        24,
        27,
        27,
        "dry",
        true,
        "Sunrise: CoolTemp=24, PowerSaving=27, Mode=dry, Fan=true"
    )]
    [InlineData(
        TimeBlock.Sunset,
        23,
        27,
        27,
        "cool",
        false,
        "Sunset: CoolTemp=23, PowerSaving=27, Mode=cool, Fan=false"
    )]
    [InlineData(
        TimeBlock.Midnight,
        22,
        25,
        25,
        "cool",
        false,
        "Midnight: CoolTemp=22, PowerSaving=25, Mode=cool, Fan=false"
    )]
    public void ClimateAutomation_TimeBlockVariations_Should_Use_Correct_Settings(
        TimeBlock timeBlock,
        int coolTemp,
        int powerSavingTemp,
        int passiveTemp,
        string mode,
        bool activateFan,
        string scenario
    )
    {
        var testSetting = new AcSettings(
            NormalTemp: 25,
            PowerSavingTemp: powerSavingTemp,
            CoolTemp: coolTemp,
            PassiveTemp: passiveTemp,
            Mode: mode,
            ActivateFan: activateFan,
            HourStart: 18,
            HourEnd: 0
        );

        SetupSchedulerMock(timeBlock, testSetting);
        _mockHaContext.ClearServiceCalls();

        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.AirConditioner.EntityId, mode);
        _mockHaContext.ClearServiceCalls();

        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        _mockHaContext.ShouldHaveCalledClimateSetTemperature(
            _entities.AirConditioner.EntityId,
            mode,
            coolTemp
        );

        scenario.Should().NotBeEmpty("Test scenario should be documented");
    }

    [Theory(Skip = "Quarantined: fan activation feature removed | issue HA-TEST-2003 | expires 2026-06-30")]
    [InlineData(true, "Fan should be activated when setting.ActivateFan is true")]
    [InlineData(false, "Fan should not be activated when setting.ActivateFan is false")]
    public void ClimateAutomation_FanActivation_Should_Follow_Setting(
        bool activateFan,
        string scenario
    )
    {
        var testSetting = new AcSettings(
            NormalTemp: 25,
            PowerSavingTemp: 27,
            CoolTemp: 23,
            PassiveTemp: 27,
            Mode: "cool",
            ActivateFan: activateFan,
            HourStart: 18,
            HourEnd: 0
        );

        SetupSchedulerMock(TimeBlock.Sunset, testSetting);

        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.AirConditioner.EntityId, "cool");
        _mockHaContext.SetEntityAttributes(
            _entities.AirConditioner.EntityId,
            new
            {
                temperature = 23.0,
                current_temperature = 28.0,
                fan_mode = "auto",
            }
        );

        _mockHaContext.ClearServiceCalls();

        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        if (activateFan)
        {
            _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.FanAutomation.EntityId);
        }
        else
        {
            _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.FanAutomation.EntityId);
        }

        scenario.Should().NotBeEmpty("Test scenario should be documented");
    }

    #endregion
}
