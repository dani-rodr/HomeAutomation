namespace HomeAutomation.apps.Area.Pantry.Automations.Entities;

public interface IPantryLightEntities : ILightAutomationEntities
{
    BinarySensorEntity MiScalePresenceSensor { get; }
    LightEntity MirrorLight { get; }
    SwitchEntity BathroomMotionAutomation { get; }
    BinarySensorEntity BathroomMotionSensor { get; }
}
