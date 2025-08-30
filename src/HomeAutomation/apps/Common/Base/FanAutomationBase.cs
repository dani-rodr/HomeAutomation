using System.Linq;

namespace HomeAutomation.apps.Common.Base;

public abstract class FanAutomationBase : ToggleableAutomation
{
    protected readonly BinarySensorEntity MotionSensor;
    protected readonly IEnumerable<SwitchEntity> Fans;
    protected readonly SwitchEntity MainFan;

    protected FanAutomationBase(IFanAutomationEntities entities, ILogger logger)
        : base(entities.MasterSwitch, logger)
    {
        MotionSensor = entities.MotionSensor;
        Fans = entities.Fans;
        MainFan = Fans.First();
    }

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        var manualOperation = MainFan.StateChanges().IsManuallyOperated();
        yield return manualOperation.WasOff().IsOn().Subscribe(_ => MasterSwitch.TurnOn());
        yield return manualOperation.WasOn().IsOff().Subscribe(_ => MasterSwitch.TurnOff());
        yield return GetMasterSwitchAutomations();
        yield return GetIdleOperationAutomations();
    }

    protected virtual void TurnOnFans(StateChange evt)
    {
        var fanIds = Fans.Select(f => f.EntityId).ToList();
        Logger.LogDebug(
            "Turning ON {FanCount} fans: [{FanIds}] - triggered by {EntityId} state change",
            Fans.Count(),
            string.Join(", ", fanIds),
            evt.Entity?.EntityId ?? "unknown"
        );

        foreach (var fan in Fans)
        {
            Logger.LogDebug("Activating fan {EntityId}", fan.EntityId);
            fan.TurnOn();
        }
    }

    protected virtual void TurnOffFans(StateChange? evt = null)
    {
        var fanIds = Fans.Select(f => f.EntityId).ToList();
        Logger.LogDebug(
            "Turning OFF {FanCount} fans: [{FanIds}] - triggered by {EntityId} state change",
            Fans.Count(),
            string.Join(", ", fanIds),
            evt?.Entity?.EntityId ?? "unknown"
        );

        foreach (var fan in Fans)
        {
            Logger.LogDebug("Deactivating fan {EntityId}", fan.EntityId);
            fan.TurnOff();
        }
    }

    protected virtual IDisposable GetIdleOperationAutomations() =>
        MotionSensor
            .StateChanges()
            .IsOff()
            .ForMinutes(15)
            .Where(_ => MasterSwitch.IsOff())
            .Subscribe(_ => MasterSwitch.TurnOn());

    private IDisposable GetMasterSwitchAutomations() =>
        MasterSwitch!
            .StateChanges()
            .IsOn()
            .Subscribe(e =>
            {
                if (MotionSensor.IsOn())
                {
                    MainFan.TurnOn();
                    return;
                }
                MainFan.TurnOff();
            });
}
