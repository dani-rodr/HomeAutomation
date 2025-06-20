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
    protected virtual bool ShouldActivateFan { get; set; }

    protected virtual void TurnOnFans(StateChange evt)
    {
        var fanIds = Fans.Select(f => f.EntityId).ToList();
        Logger.LogDebug(
            "Turning ON {FanCount} fans: [{FanIds}] - triggered by {EntityId} state change",
            Fans.Length,
            string.Join(", ", fanIds),
            evt.Entity?.EntityId ?? "unknown"
        );

        Fans.ToList()
            .ForEach(fan =>
            {
                Logger.LogDebug("Activating fan {EntityId}", fan.EntityId);
                fan.TurnOn();
            });
    }

    protected virtual void TurnOffFans(StateChange evt)
    {
        var fanIds = Fans.Select(f => f.EntityId).ToList();
        Logger.LogDebug(
            "Turning OFF {FanCount} fans: [{FanIds}] - triggered by {EntityId} state change",
            Fans.Length,
            string.Join(", ", fanIds),
            evt.Entity?.EntityId ?? "unknown"
        );

        Fans.ToList()
            .ForEach(fan =>
            {
                Logger.LogDebug("Deactivating fan {EntityId}", fan.EntityId);
                fan.TurnOff();
            });
    }
}
