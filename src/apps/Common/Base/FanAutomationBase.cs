using System.Linq;

namespace HomeAutomation.apps.Common.Base;

public abstract class FanAutomationBase(
    SwitchEntity masterSwitch,
    BinarySensorEntity motionSensor,
    ILogger logger,
    params SwitchEntity[] fans
) : AutomationBase(logger, masterSwitch)
{
    protected readonly BinarySensorEntity MotionSensor = motionSensor;
    protected readonly SwitchEntity[] Fans = fans;
    protected readonly SwitchEntity Fan = fans.First();
    protected abstract bool ShouldActivateFan { get; set; }

    protected virtual void TurnOnFans(StateChange _) => Fans.ToList().ForEach(fan => fan.TurnOn());

    protected virtual void TurnOffFans(StateChange _) => Fans.ToList().ForEach(fan => fan.TurnOff());
}
