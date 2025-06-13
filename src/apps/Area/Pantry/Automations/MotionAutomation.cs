using System.Collections.Generic;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.apps.Area.Pantry.Automations;

public class MotionAutomation(IPantryMotionEntities entities, ILogger logger)
    : MotionAutomationBase(entities.MasterSwitch, entities.MotionSensor, entities.Light, logger, entities.SensorDelay)
{
    private readonly BinarySensorEntity _miScalePresenceSensor = entities.MiScalePresenceSensor;
    private readonly LightEntity _mirrorLight = entities.MirrorLight;
    private readonly BinarySensorEntity _roomDoor = entities.RoomDoor;

    protected override int SensorWaitTime => 10;

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [_roomDoor.StateChanges().IsOff().Subscribe(_ => MasterSwitch?.TurnOn())];

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        // Lighting automation
        yield return MotionSensor.StateChanges().IsOn().Subscribe(_ => Light.TurnOn());
        yield return MotionSensor
            .StateChanges()
            .IsOff()
            .Subscribe(_ =>
            {
                Light.TurnOff();
                _mirrorLight.TurnOff();
            });
        yield return _miScalePresenceSensor.StateChanges().IsOn().Subscribe(_ => _mirrorLight.TurnOn());
    }
}
