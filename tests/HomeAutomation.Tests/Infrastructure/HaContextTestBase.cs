namespace HomeAutomation.Tests.Infrastructure;

public abstract class HaContextTestBase : IDisposable
{
    protected MockHaContext HaContext { get; } = new();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            HaContext.Dispose();
        }
    }
}
