namespace HomeAutomation.apps.Common.Base;

public abstract class MotionAutomationBase(IMotionAutomationEntities entities, ILogger logger)
    : AutomationBase(logger, entities.MasterSwitch)
{
    protected readonly BinarySensorEntity MotionSensor = entities.MotionSensor;
    protected readonly NumberEntity? SensorDelay = entities.SensorDelay;
    protected readonly LightEntity Light = entities.Light;
    protected virtual int SensorWaitTime => 15;
    protected virtual int SensorActiveDelayValue => 5;
    protected virtual int SensorInactiveDelayValue => 1;

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return Light
            .StateChanges()
            .IsManuallyOperated()
            .Subscribe(ControlMasterSwitchOnLightChange);
        if (MasterSwitch != null)
        {
            yield return MasterSwitch.StateChanges().IsOn().Subscribe(ControlLightOnMotionChange);
        }
        foreach (var automation in GetAdditionalPersistentAutomations())
        {
            yield return automation;
        }
    }

    protected override IEnumerable<IDisposable> GetToggleableAutomations() =>
        [
            .. GetLightAutomations(),
            .. GetSensorDelayAutomations(),
            .. GetAdditionalSwitchableAutomations(),
        ];

    protected virtual IEnumerable<IDisposable> GetLightAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetAdditionalSwitchableAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetAdditionalPersistentAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetSensorDelayAutomations()
    {
        if (SensorDelay == null)
        {
            Logger.LogDebug(
                "No sensor delay entity configured for {AutomationType}",
                GetType().Name
            );
            yield break;
        }

        Logger.LogDebug(
            "Configuring sensor delay automations: ActiveValue={ActiveValue}, InactiveValue={InactiveValue}, WaitTime={WaitTime}s",
            SensorActiveDelayValue,
            SensorInactiveDelayValue,
            SensorWaitTime
        );

        yield return MotionSensor
            .StateChanges()
            .IsOnForSeconds(SensorWaitTime)
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Motion sustained for {WaitTime}s - setting sensor delay to active value {Value}",
                    SensorWaitTime,
                    SensorActiveDelayValue
                );
                SensorDelay.SetNumericValue(SensorActiveDelayValue);
            });
        yield return MotionSensor
            .StateChanges()
            .IsOffForSeconds(SensorWaitTime)
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Motion cleared for {WaitTime}s - setting sensor delay to inactive value {Value}",
                    SensorWaitTime,
                    SensorInactiveDelayValue
                );
                SensorDelay.SetNumericValue(SensorInactiveDelayValue);
            });
        yield return MotionSensor
            .StateChanges()
            .IsFlickering()
            .Subscribe(events =>
            {
                Logger.LogDebug(
                    "Motion sensor flickering detected ({EventCount} events) - setting sensor delay to active value {Value}",
                    events.Count,
                    SensorActiveDelayValue
                );
                SensorDelay.SetNumericValue(SensorActiveDelayValue);
            });
    }

    private void ControlMasterSwitchOnLightChange(StateChange evt)
    {
        var lightState = Light.IsOn();
        var motionState = MotionSensor.IsOccupied();

        Logger.LogDebug(
            "LightChange detected: Light.IsOn={Light}, MotionSensor.IsOccupied={Motion}",
            lightState,
            motionState
        );

        if (lightState == motionState)
        {
            Logger.LogDebug("Enabling automation via MasterSwitch (states match)");
            MasterSwitch?.TurnOn();
            return;
        }

        Logger.LogDebug("Disabling automation via MasterSwitch (states mismatch)");
        MasterSwitch?.TurnOff();
    }

    private void ControlLightOnMotionChange(StateChange evt)
    {
        Logger.LogDebug(
            "Motion state changed: {OldState} → {NewState} for {EntityId} by {UserId}",
            evt.Old?.State,
            evt.New?.State,
            MotionSensor.EntityId,
            evt.Username() ?? "unknown"
        );

        if (MotionSensor.IsOn())
        {
            Logger.LogDebug("Motion detected - turning on light {EntityId}", Light.EntityId);
            Light.TurnOn();
            return;
        }

        Logger.LogDebug("Motion cleared - turning off light {EntityId}", Light.EntityId);
        Light.TurnOff();
    }
}
