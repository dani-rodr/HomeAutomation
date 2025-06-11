using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(Entities entities, ILogger<LivingRoomApp> logger) : AreaBase<LivingRoomApp>(entities, logger)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        var standFan = Entities.Switch.Sonoff10023810231;
        var motionSensorSwitch = Entities.Switch.SalaMotionSensor;
        var motionSensor = Entities.BinarySensor.LivingRoomPresenceSensors;

        yield return new MotionAutomation(Entities, motionSensorSwitch, motionSensor, Logger);
        yield return new FanAutomation(Entities, motionSensorSwitch, motionSensor, standFan, Logger);
        yield return new AirQualityAutomations(Entities, standFan, Logger);
        yield return new TabletAutomations(Entities, motionSensorSwitch, motionSensor, Logger);
    }
}
