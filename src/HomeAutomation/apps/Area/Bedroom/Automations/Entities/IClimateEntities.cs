namespace HomeAutomation.apps.Area.Bedroom.Automations.Entities;

public interface IClimateEntities : IMotionBase
{
    ClimateEntity AirConditioner { get; }
    BinarySensorEntity Door { get; }
    BinarySensorEntity HouseMotionSensor { get; }
    ButtonEntity AcFanModeToggle { get; }
    SwitchEntity Fan { get; }
    InputBooleanEntity PowerSavingMode { get; }
}
