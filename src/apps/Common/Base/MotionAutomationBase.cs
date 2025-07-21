namespace HomeAutomation.apps.Common.Base;

/// <summary>
/// Base class for motion-based automations that provides sensor delay management and motion patterns.
/// Acts as the automation layer above MotionSensor (device layer).
/// </summary>
public class MotionAutomationBase(MotionSensor motionSensor, ILogger logger)
    : AutomationBase(motionSensor.GetMasterSwitch(), logger)
{
    protected readonly MotionSensor MotionSensor = motionSensor;
    public virtual int SensorWaitTime => 15;
    public virtual int SensorActiveDelayValue => 5;
    public virtual int SensorInactiveDelayValue => 1;

    protected override IEnumerable<IDisposable> GetPersistentAutomations() => [];

    protected override IEnumerable<IDisposable> GetToggleableAutomations() =>
        [.. GetSensorDelayAutomations()];

    protected virtual IEnumerable<IDisposable> GetSensorDelayAutomations()
    {
        var motionSensorEntity = MotionSensor.MotionSensorEntity;
        var sensorDelay = MotionSensor.SensorDelay;

        Logger.LogDebug(
            "Configuring motion sensor delay automations: ActiveValue={ActiveValue}, InactiveValue={InactiveValue}, WaitTime={WaitTime}s",
            SensorActiveDelayValue,
            SensorInactiveDelayValue,
            SensorWaitTime
        );

        yield return motionSensorEntity
            .StateChanges()
            .IsOnForSeconds(SensorWaitTime)
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Motion sustained for {WaitTime}s - setting sensor delay to active value {Value}",
                    SensorWaitTime,
                    SensorActiveDelayValue
                );
                sensorDelay.SetNumericValue(SensorActiveDelayValue);
            });
        yield return motionSensorEntity
            .StateChanges()
            .IsOffForSeconds(SensorWaitTime)
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Motion cleared for {WaitTime}s - setting sensor delay to inactive value {Value}",
                    SensorWaitTime,
                    SensorInactiveDelayValue
                );
                sensorDelay.SetNumericValue(SensorInactiveDelayValue);
            });
        yield return motionSensorEntity
            .StateChanges()
            .IsFlickering()
            .Subscribe(events =>
            {
                Logger.LogDebug(
                    "Motion sensor flickering detected ({EventCount} events) - setting sensor delay to active value {Value}",
                    events.Count,
                    SensorActiveDelayValue
                );
                sensorDelay.SetNumericValue(SensorActiveDelayValue);
            });
    }

    /// <summary>
    /// Access to the motion sensor entity for derived classes or external automations
    /// </summary>
    public BinarySensorEntity GetMotionSensor() => MotionSensor.MotionSensorEntity;

    /// <summary>
    /// Access to the sensor delay entity for derived classes or external automations
    /// </summary>
    public NumberEntity GetSensorDelay() => MotionSensor.SensorDelay;

    /// <summary>
    /// Access to the restart button entity for derived classes or external automations
    /// </summary>
    public ButtonEntity GetRestartButton() => MotionSensor.Restart;
}
