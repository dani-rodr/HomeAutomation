using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(Entities entities, ILogger<LivingRoomApp> logger) : AreaBase<LivingRoomApp>(entities, logger)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        var standFan = Entities.Switch.Sonoff10023810231;
        var motionSensorSwitch = Entities.Switch.SalaMotionSensor;
        var motionSensor = Entities.BinarySensor.LivingRoomPresenceSensors;

        var motionEntities = new LivingRoomMotionEntities(Entities);
        yield return new MotionAutomation(motionEntities, Logger);

        var fanEntities = new LivingRoomFanEntities(Entities, motionSensorSwitch, motionSensor, standFan);
        yield return new FanAutomation(fanEntities, Logger);

        var airQualityEntities = new AirQualityEntities(Entities, standFan);
        yield return new AirQualityAutomations(airQualityEntities, Logger);

        var tabletEntities = new LivingRoomTabletEntities(Entities, motionSensorSwitch, motionSensor);
        yield return new TabletAutomations(tabletEntities, Logger);
    }
}
