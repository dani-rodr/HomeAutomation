namespace HomeAutomation.apps.Common.Base;

public abstract class LightAutomationBase(
    ILightAutomationEntities entities,
    MotionAutomationBase motionAutomation,
    ILogger logger
) : AutomationBase(entities.MasterSwitch, logger)
{
    protected readonly MotionAutomationBase MotionAutomation = motionAutomation;
    protected readonly LightEntity Light = entities.Light;

    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [
            Light.StateChanges().IsManuallyOperated().Subscribe(ControlMasterSwitchOnLightChange),
            MasterSwitch!.StateChanges().IsOn().Subscribe(ControlLightOnMotionChange),
            .. GetAdditionalPersistentAutomations(),
        ];

    protected override IEnumerable<IDisposable> GetToggleableAutomations() =>
        [.. GetLightAutomations(), .. GetAdditionalSwitchableAutomations()];

    protected virtual IEnumerable<IDisposable> GetLightAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetAdditionalSwitchableAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetAdditionalPersistentAutomations() => [];

    private void ControlMasterSwitchOnLightChange(StateChange evt)
    {
        var lightState = Light.IsOn();
        var motionState = MotionAutomation.GetMotionSensor().IsOccupied();

        Logger.LogDebug(
            "LightChange detected: Light.IsOn={Light}, MotionSensor.IsOccupied={Motion}",
            lightState,
            motionState
        );

        if (lightState == motionState)
        {
            Logger.LogDebug("Enabling automation via MasterSwitch (states match)");
            MasterSwitch.TurnOn();
            return;
        }

        Logger.LogDebug("Disabling automation via MasterSwitch (states mismatch)");
        MasterSwitch.TurnOff();
    }

    private void ControlLightOnMotionChange(StateChange evt)
    {
        Logger.LogDebug(
            "Motion state changed: {OldState} → {NewState} for {EntityId} by {UserId}",
            evt.Old?.State,
            evt.New?.State,
            MotionAutomation.GetMotionSensor().EntityId,
            evt.Username() ?? "unknown"
        );

        if (MotionAutomation.GetMotionSensor().IsOn())
        {
            Logger.LogDebug("Motion detected - turning on light {EntityId}", Light.EntityId);
            Light.TurnOn();
            return;
        }

        Logger.LogDebug("Motion cleared - turning off light {EntityId}", Light.EntityId);
        Light.TurnOff();
    }
}
