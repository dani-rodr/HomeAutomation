using System.Linq;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class FanAutomation(ILivingRoomFanEntities entities, ILogger<FanAutomation> logger)
    : FanAutomationBase(entities, logger)
{
    private SwitchEntity ExhaustFan => Fans.Last();

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
        yield return MotionSensor.StateChanges().IsOnForSeconds(3).Subscribe(TurnOnFans);
        yield return MotionSensor.StateChanges().IsOffForMinutes(1).Subscribe(TurnOffFans);
    }
}
