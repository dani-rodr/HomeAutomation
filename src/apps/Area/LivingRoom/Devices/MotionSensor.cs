using HomeAutomation.apps.Common.Base;

namespace HomeAutomation.apps.Area.LivingRoom.Devices;

public class MotionSensor(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    ILogger<MotionSensor> logger
) : Common.Base.MotionSensor(factory, scheduler, "living_room_motion_sensor", logger);