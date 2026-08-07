namespace HomeAssistantGenerated;

// These app-facing aliases preserve existing feature contracts for entities that
// are currently absent from the HA registry and therefore omitted by nd-codegen.
public partial class SwitchEntities
{
    public SwitchEntity Cozylife955f => new(_haContext, "switch.cozylife_955f");
    public SwitchEntity Laptop => new(_haContext, "switch.laptop");
}

public partial class SensorEntities
{
    public SensorEntity Thinkpadt14Sessionstate =>
        new(_haContext, "sensor.thinkpadt14_sessionstate");

    public NumericSensorEntity Thinkpadt14BatteryChargeRemainingPercentage =>
        new(_haContext, "sensor.thinkpadt14_battery_charge_remaining_percentage");
}

public partial class ButtonEntities
{
    public ButtonEntity Thinkpadt14Lock => new(_haContext, "button.thinkpadt14_lock");
    public ButtonEntity Thinkpadt14Sleep => new(_haContext, "button.thinkpadt14_sleep");
}

public partial class InputBooleanEntities
{
    public InputBooleanEntity ProjectNationWeek =>
        new(_haContext, "input_boolean.project_nation_week");
}
