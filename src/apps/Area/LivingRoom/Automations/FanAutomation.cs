namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class FanAutomation(ILivingRoomFanEntities entities, ILogger logger)
    : FanAutomationBase(entities.MasterSwitch, entities.MotionSensor, logger, [.. entities.Fans])
{
    private SwitchEntity ExhaustFan => Fans[2];

    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [Fan.StateChanges().IsManuallyOperated().Subscribe(ControlMasterSwitchOnFanChange)];

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        Logger.LogInformation("Living Room Fan Automation initialized");
        return [.. GetSalaFanAutomations()];
    }

    private void TurnOnSalaFans(StateChange e)
    {
        Logger.LogDebug(
            "Motion detected on {MotionSensor}. Evaluating fan activation logic, BedroomMotion: {BedroomState}",
            e.Entity?.EntityId ?? "unknown",
            entities.BedroomMotionSensor.State
        );

        Fan.TurnOn();

        if (entities.BedroomMotionSensor.IsOff())
        {
            Logger.LogDebug(
                "Bedroom motion sensor {EntityId} is OFF - activating exhaust fan {ExhaustFanId}",
                entities.BedroomMotionSensor.EntityId,
                ExhaustFan.EntityId
            );
            ExhaustFan.TurnOn();
        }
    }

    private IEnumerable<IDisposable> GetSalaFanAutomations()
    {
        yield return MotionSensor.StateChanges().IsOnForSeconds(3).Subscribe(TurnOnSalaFans);
        yield return MotionSensor.StateChanges().IsOffForMinutes(1).Subscribe(TurnOffFans);
        yield return MotionSensor
            .StateChanges()
            .IsOffForMinutes(15)
            .Where(_ => MasterSwitch.IsOff() == true)
            .Subscribe(_ => MasterSwitch?.TurnOn());
    }

    private void ControlMasterSwitchOnFanChange(StateChange change)
    {
        var fanState = Fan.IsOn();
        var motionState = MotionSensor.IsOccupied();

        Logger.LogDebug(
            "FanChange detected: Fan.IsOn={Fan}, MotionSensor.IsOccupied={Motion}",
            fanState,
            motionState
        );

        if (fanState == motionState)
        {
            Logger.LogDebug("Enabling automation via MasterSwitch (states match)");
            MasterSwitch?.TurnOn();
            return;
        }

        Logger.LogDebug("Disabling automation via MasterSwitch (states mismatch)");
        MasterSwitch?.TurnOff();
    }
}
