namespace HomeAutomation.apps.Area.Pantry.Automations;

public interface IPantryLightEntities : ILightAutomationEntities
{
    BinarySensorEntity MiScalePresenceSensor { get; }
    LightEntity MirrorLight { get; }
    SwitchEntity BathroomMotionAutomation { get; }
    BinarySensorEntity BathroomMotionSensor { get; }
}
