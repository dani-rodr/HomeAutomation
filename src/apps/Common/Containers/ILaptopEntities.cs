namespace HomeAutomation.apps.Common.Containers;

public interface ILaptopEntities
{
    SwitchEntity Switch { get; }
    ButtonEntity[] WakeOnLanButtons { get; }
    SwitchEntity PowerPlug { get; }
    SensorEntity Session { get; }
    NumericSensorEntity BatteryLevel { get; }
    ButtonEntity Lock { get; }
}

public class LaptopEntities(Entities entities) : ILaptopEntities
{
    public SwitchEntity Switch => entities.Switch.Laptop;

    public ButtonEntity[] WakeOnLanButtons =>
        [entities.Button.Thinkpadt14WakeOnLan, entities.Button.Thinkpadt14WakeOnWlan];
    public SwitchEntity PowerPlug => entities.Switch.Sonoff1002380fe51;

    public SensorEntity Session => entities.Sensor.Thinkpadt14Sessionstate;

    public NumericSensorEntity BatteryLevel => entities.Sensor.Thinkpadt14BatteryChargeRemainingPercentage;
    public ButtonEntity Lock => entities.Button.Thinkpadt14Lock;
}
