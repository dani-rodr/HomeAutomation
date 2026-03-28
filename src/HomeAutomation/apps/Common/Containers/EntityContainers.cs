namespace HomeAutomation.apps.Common.Containers;

public class ClimateSchedulerEntities(Devices devices) : IClimateSchedulerEntities
{
    private readonly WeatherControl _control = devices.Global.WeatherControl!;
    public SensorEntity SunRising => _control.SunRising;
    public SensorEntity SunSetting => _control.SunSetting;
    public SensorEntity SunMidnight => _control.SunMidnight;
    public WeatherEntity Weather => _control!.Weather;
    public InputBooleanEntity PowerSavingMode => _control.PowerSavingMode;
}
