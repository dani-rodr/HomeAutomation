using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.Tests.Infrastructure;

public abstract class AutomationTestBase<TAutomation> : IDisposable
    where TAutomation : class, IAutomation, IDisposable
{
    protected MockHaContext HaContext { get; } = new();

    protected Mock<ILogger<TAutomation>> Logger { get; } = new();

    protected void StartAutomation(TAutomation automation, string? masterSwitchEntityId = null)
    {
        automation.StartAutomation();

        if (masterSwitchEntityId is not null)
        {
            HaContext.SimulateStateChange(masterSwitchEntityId, "off", "on");
        }

        HaContext.ClearServiceCalls();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        HaContext.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
