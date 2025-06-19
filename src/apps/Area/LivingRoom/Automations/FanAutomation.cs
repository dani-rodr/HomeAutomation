namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class FanAutomation(ILivingRoomFanEntities entities, ILogger logger)
    : FanAutomationBase(entities.MasterSwitch, entities.MotionSensor, logger, [.. entities.Fans])
{
    protected override bool ShouldActivateFan { get; set; } = false;
    private SwitchEntity ExhaustFan => Fans[2];

    protected override IEnumerable<IDisposable> GetPersistentAutomations() => [];

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        Logger.LogInformation("Living Room Fan Automation initialized");
        return [.. GetSalaFanAutomations(), .. GetCeilingFanManualOperations()];
    }

    private IEnumerable<IDisposable> GetCeilingFanManualOperations()
    {
        yield return Fan.StateChanges()
            .IsManuallyOperated()
            .Subscribe(e =>
            {
                var newValue = e.IsOn();
                Logger.LogDebug(
                    "Manual fan operation detected on {EntityId}: {OldState} → {NewState}. Setting ShouldActivateFan = {ShouldActivate}",
                    e.Entity?.EntityId ?? "unknown",
                    e.Old?.State ?? "unknown",
                    e.New?.State ?? "unknown",
                    newValue
                );
                ShouldActivateFan = newValue;
            });

        yield return Fan.StateChanges()
            .IsOffForMinutes(15)
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Fan {EntityId} has been OFF for 15 minutes. Resetting ShouldActivateFan = true to allow automation",
                    Fan.EntityId
                );
                ShouldActivateFan = true;
            });
    }

    private void TurnOnSalaFans(StateChange e)
    {
        Logger.LogDebug(
            "Motion detected on {MotionSensor}. Evaluating fan activation logic - ShouldActivateFan: {ShouldActivate}, BedroomMotion: {BedroomState}",
            e.Entity?.EntityId ?? "unknown",
            ShouldActivateFan,
            entities.BedroomMotionSensor.State
        );

        if (ShouldActivateFan)
        {
            Logger.LogDebug(
                "Activating main ceiling fan {EntityId} due to motion and ShouldActivateFan=true",
                Fan.EntityId
            );
            Fan.TurnOn();
        }
        else
        {
            Logger.LogDebug(
                "Skipping main ceiling fan {EntityId} activation - ShouldActivateFan=false (likely due to manual override)",
                Fan.EntityId
            );
        }

        if (entities.BedroomMotionSensor.IsOff())
        {
            Logger.LogDebug(
                "Bedroom motion sensor {EntityId} is OFF - activating exhaust fan {ExhaustFanId}",
                entities.BedroomMotionSensor.EntityId,
                ExhaustFan.EntityId
            );
            ExhaustFan.TurnOn();
        }
        else
        {
            Logger.LogDebug(
                "Bedroom motion sensor {EntityId} is ON - skipping exhaust fan {ExhaustFanId} activation",
                entities.BedroomMotionSensor.EntityId,
                ExhaustFan.EntityId
            );
        }
    }

    private IEnumerable<IDisposable> GetSalaFanAutomations()
    {
        yield return MotionSensor
            .StateChanges()
            .IsOnForSeconds(3)
            .Subscribe(e =>
            {
                Logger.LogDebug(
                    "Motion sensor {EntityId} has been ON for 3 seconds - triggering fan activation logic",
                    e.Entity?.EntityId ?? "unknown"
                );
                TurnOnSalaFans(e);
            });

        yield return MotionSensor
            .StateChanges()
            .IsOffForMinutes(1)
            .Subscribe(e =>
            {
                Logger.LogDebug(
                    "Motion sensor {EntityId} has been OFF for 1 minute - turning off all fans",
                    e.Entity?.EntityId ?? "unknown"
                );
                TurnOffFans(e);
            });
    }
}
