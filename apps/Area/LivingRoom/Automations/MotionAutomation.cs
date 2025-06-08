using System.Collections.Generic;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class MotionAutomation(Entities entities, ILogger<LivingRoom> logger)
    : DimmingMotionAutomationBase(
        entities.Switch.SalaMotionSensor,
        entities.BinarySensor.LivingRoomPresenceSensors,
        entities.Light.SalaLightsGroup,
        entities.Number.Ld2410Esp321StillTargetDelay,
        logger
    )
{
    protected override int SensorWaitTime => 30;
    protected override int SensorDelayValueActive => 45;
    protected override int SensorDelayValueInactive => 1;
    protected override int DimBrightnessPct => 80;
    protected override int DimDelaySeconds => 15;
    private readonly SwitchEntity _ceilingFan = entities.Switch.CeilingFan;
    private readonly SwitchEntity _standFan = entities.Switch.Sonoff10023810231;
    private readonly SwitchEntity _exhaustFan = entities.Switch.Cozylife955f;

    public override void StartAutomation()
    {
        base.StartAutomation();
        MotionSensor
            .StateChanges()
            .IsOffForMinutes(30)
            .Where(_ => entities.MediaPlayer.Tcl65c755.IsOff())
            .Subscribe(_ => MasterSwitch?.TurnOn());
    }

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        return [.. base.GetLightAutomations(), AutoPantryOffWithSala()];
    }

    protected override IEnumerable<IDisposable> GetAdditionalSwitchableAutomations()
    {
        return
        [
            .. GetSalaFanAutomations(),
            entities
                .BinarySensor.KitchenMotionSensors.StateChanges()
                .IsOnForSeconds(10)
                .Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueActive)),
        ];
    }

    private IEnumerable<IDisposable> GetSalaFanAutomations() // Better create a fan Automation class with the air purifier
    {
        yield return MotionSensor.StateChanges().IsOnForSeconds(3).Subscribe(TurnOnSalaFans);
        yield return MotionSensor.StateChanges().IsOffForMinutes(1).Subscribe(TurnOffAllFans);
    }

    private void TurnOnSalaFans(StateChange e)
    {
        _ceilingFan.TurnOn();
        if (entities.BinarySensor.BedroomPresenceSensors.IsOff())
        {
            _exhaustFan.TurnOn();
        }
    }

    private void TurnOffAllFans(StateChange e)
    {
        _ceilingFan.TurnOff();
        _standFan.TurnOff();
        _exhaustFan.TurnOff();
    }

    private IDisposable AutoPantryOffWithSala()
    {
        return Light
            .StateChanges()
            .IsOff()
            .Where(_ => ShouldPantryLightsTurnOff())
            .Subscribe(_ => entities.Light.PantryLights.TurnOff());
    }

    private bool ShouldPantryLightsTurnOff() =>
        entities.Switch.PantryMotionSensor.IsOn() && entities.BinarySensor.PantryMotionSensors.IsOff();
}
