namespace HomeAutomation.apps.Common.Containers;

public interface ICookingAutomationEntities
{
    NumericSensorEntity RiceCookerPower { get; }
    SwitchEntity RiceCookerSwitch { get; }
    SensorEntity AirFryerStatus { get; }
    ButtonEntity InductionTurnOff { get; }
    NumericSensorEntity InductionPower { get; }
}

public class KitchenCookingEntities(Entities entities) : ICookingAutomationEntities
{
    public NumericSensorEntity RiceCookerPower => entities.Sensor.RiceCookerPower;
    public SwitchEntity RiceCookerSwitch => entities.Switch.RiceCookerSocket1;
    public SensorEntity AirFryerStatus => entities.Sensor.CareliSg593061393Maf05aStatusP21;
    public ButtonEntity InductionTurnOff => entities.Button.InductionCookerPower;
    public NumericSensorEntity InductionPower => entities.Sensor.SmartPlug3SonoffS31Power;
}