namespace HomeAutomation.apps.Common.Base;

public abstract class MotionSensorBase(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    string deviceName,
    ILogger logger
) : ToggleableAutomation(factory.Create<SwitchEntity>(deviceName, "auto_calibrate"), logger)
{
    private readonly MotionSensorCore _core = new(factory, scheduler, deviceName, logger);

    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        _core.GetPersistentAutomations(MasterSwitch);

    protected override IEnumerable<IDisposable> GetToggleableAutomations() =>
        _core.GetToggleableAutomations();
}
