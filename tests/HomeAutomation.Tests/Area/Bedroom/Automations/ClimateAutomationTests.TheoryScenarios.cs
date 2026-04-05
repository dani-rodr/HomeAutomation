using HomeAutomation.apps.Area.Bedroom.Config;

namespace HomeAutomation.Tests.Area.Bedroom.Automations;

public partial class ClimateAutomationTests
{
    #region Comprehensive Theory Tests for Temperature Selection

    [Theory]
    [InlineData(true, false, TimeBlock.Sunset, 23, "cool", "Occupied + closed door = ComfortTemp")]
    [InlineData(false, true, TimeBlock.Sunset, 25, "cool", "Unoccupied + open door = AwayTemp")]
    [InlineData(true, true, TimeBlock.Sunset, 24, "cool", "Occupied + open door = DoorOpenTemp")]
    [InlineData(false, false, TimeBlock.Sunset, 25, "cool", "Unoccupied + closed door = AwayTemp")]
    public void ClimateAutomation_TemperatureSelection_Should_Follow_Logic(
        bool occupied,
        bool doorOpen,
        TimeBlock timeBlock,
        int expectedTemp,
        string expectedMode,
        string scenario
    )
    {
        var testSetting = new ClimateSetting(24, 23, 25, expectedMode, 18, 0);

        SetupSchedulerMock(timeBlock, testSetting);
        _mockHaContext.ClearServiceCalls();

        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, occupied ? "on" : "off");
        _mockHaContext.SetEntityState(_entities.Door.EntityId, doorOpen ? "on" : "off");
        _mockHaContext.SetEntityState(_entities.AirConditioner.EntityId, "cool");
        _mockHaContext.ClearServiceCalls();

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        _mockHaContext.ShouldHaveCalledClimateSetTemperature(
            _entities.AirConditioner.EntityId,
            expectedTemperature: expectedTemp
        );

        scenario.Should().NotBeEmpty("Test scenario should be documented");
    }

    [Theory]
    [InlineData(
        TimeBlock.Sunrise,
        24,
        25,
        25,
        "dry",
        "Sunrise: ComfortTemp=24, AwayTemp=25, Mode=dry, Fan=true"
    )]
    [InlineData(
        TimeBlock.Sunset,
        23,
        24,
        25,
        "cool",
        "Sunset: ComfortTemp=23, AwayTemp=25, Mode=cool, Fan=false"
    )]
    [InlineData(
        TimeBlock.Midnight,
        22,
        24,
        25,
        "cool",
        "Midnight: ComfortTemp=22, AwayTemp=25, Mode=cool, Fan=false"
    )]
    public void ClimateAutomation_TimeBlockVariations_Should_Use_Correct_Settings(
        TimeBlock timeBlock,
        int coolTemp,
        int powerSavingTemp,
        int passiveTemp,
        string mode,
        string scenario
    )
    {
        var testSetting = new ClimateSetting(powerSavingTemp, coolTemp, passiveTemp, mode, 18, 0);

        SetupSchedulerMock(timeBlock, testSetting);
        _mockHaContext.ClearServiceCalls();

        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.AirConditioner.EntityId, mode);
        _mockHaContext.ClearServiceCalls();

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        _mockHaContext.ShouldHaveCalledClimateSetTemperature(
            _entities.AirConditioner.EntityId,
            expectedTemperature: coolTemp
        );

        scenario.Should().NotBeEmpty("Test scenario should be documented");
    }

    #endregion
}
