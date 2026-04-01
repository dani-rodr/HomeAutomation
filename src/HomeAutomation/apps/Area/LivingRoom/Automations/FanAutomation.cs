using System.Reactive.Disposables;
using HomeAutomation.apps.Area.LivingRoom.Automations.Entities;
using HomeAutomation.apps.Area.LivingRoom.Config;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class FanAutomation(
    ILivingRoomFanEntities entities,
    LivingRoomFanSettings settings,
    ILogger<FanAutomation> logger
) : FanAutomationBase(entities, logger)
{
    private readonly SwitchEntity ExhaustFan = entities.ExhaustFan;
    private LivingRoomFanSettings Settings => settings;

    protected override IEnumerable<IDisposable> GetToggleableAutomations() =>
        [.. GetSalaFanAutomations()];

    protected override void TurnOnFans(StateChange e)
    {
        Logger.LogDebug(
            "Motion detected on {MotionSensor}. Evaluating fan activation logic, BedroomMotion: {BedroomState}",
            e.Entity?.EntityId ?? "unknown",
            entities.BedroomMotionSensor.State
        );

        MainFan.TurnOn();

        if (entities.BedroomMotionSensor.IsClear())
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
        yield return MotionSensor
            .OnOccupied(new(Seconds: Settings.MotionOnDelaySeconds))
            .Subscribe(TurnOnFans);

        yield return MotionSensor
            .OnCleared(new(Minutes: Settings.MotionOffDelayMinutes))
            .Subscribe(TurnOffFans);
    }

    protected override IDisposable GetIdleOperationAutomations() => Disposable.Empty;

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return GetMasterSwitchAutomations();
    }
}
