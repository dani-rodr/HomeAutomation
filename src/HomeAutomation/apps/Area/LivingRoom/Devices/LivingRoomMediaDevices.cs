namespace HomeAutomation.apps.Area.LivingRoom.Devices;

public class LivingRoomMediaDevices(HomeAssistantGenerated.Entities entities)
{
    public BinarySensorEntity MotionSensor { get; } =
        entities.BinarySensor.LivingRoomPresenceSensors;
    public ButtonEntity Restart { get; } = entities.Button.SalaMotionSensorRestartEsp32;
    public NumberEntity SensorDelay { get; } = entities.Number.SalaMotionSensorStillTargetDelay;
    public MediaPlayerEntity TclTv { get; } = entities.MediaPlayer.Tcl65c755;
    public SwitchEntity TvAutomation { get; } = entities.Switch.TvAutomation;
    public LightEntity TvBacklight { get; } = entities.Light.TvBacklight3Lite;
    public LightEntity TabletLight { get; } = entities.Light.MipadScreen;
    public BinarySensorEntity TabletActive { get; } = entities.BinarySensor.Mipad;
}
