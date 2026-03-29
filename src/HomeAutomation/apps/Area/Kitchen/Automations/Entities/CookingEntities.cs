using HomeAutomation.apps.Area.Kitchen.Devices;

namespace HomeAutomation.apps.Area.Kitchen.Automations.Entities;

public class CookingEntities(KitchenDevices devices) : ICookingEntities
{
    public SwitchEntity MasterSwitch => devices.CookingAutomation;
    public NumericSensorEntity RiceCookerPower => devices.RiceCookerPower;
    public SwitchEntity RiceCookerSwitch => devices.RiceCookerSwitch;
    public SensorEntity AirFryerStatus => devices.AirFryerStatus;
    public TimerEntity AirFryerTimer => devices.AirFryerTimer;
    public ButtonEntity InductionTurnOff => devices.InductionTurnOff;
    public NumericSensorEntity InductionPower => devices.InductionPower;
}
