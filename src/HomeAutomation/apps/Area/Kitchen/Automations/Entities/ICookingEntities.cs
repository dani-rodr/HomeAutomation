namespace HomeAutomation.apps.Area.Kitchen.Automations.Entities;

public interface ICookingEntities
{
    SwitchEntity MasterSwitch { get; }
    NumericSensorEntity RiceCookerPower { get; }
    SwitchEntity RiceCookerSwitch { get; }
    SensorEntity AirFryerStatus { get; }
    TimerEntity AirFryerTimer { get; }
    ButtonEntity InductionTurnOff { get; }
    NumericSensorEntity InductionPower { get; }
}
