using HomeAutomation.apps.Common.Base;

namespace HomeAutomation.Tests.Common.Base;

public class AutomationBaseTests
{
    private readonly Mock<ILogger> _logger = new();

    [Fact]
    public void StartAutomation_WhenGetAutomationsThrowsAfterYield_ShouldDisposeCreatedDisposables()
    {
        // Arrange
        var firstDisposable = new DisposableSpy();
        var sut = new ScriptedAutomation(_logger.Object, [() => FailingSequence(firstDisposable)]);

        // Act
        var act = () => sut.StartAutomation();

        // Assert
        act.Should().Throw<InvalidOperationException>();
        firstDisposable.DisposeCount.Should().Be(1);
    }

    [Fact]
    public void StartAutomation_AfterFailedStart_ShouldNotDoubleDisposeRolledBackDisposables()
    {
        // Arrange
        var rolledBackDisposable = new DisposableSpy();
        var activeDisposable = new DisposableSpy();
        var sut = new ScriptedAutomation(
            _logger.Object,
            [() => FailingSequence(rolledBackDisposable), () => [activeDisposable]]
        );

        // Act
        var firstStart = () => sut.StartAutomation();
        firstStart.Should().Throw<InvalidOperationException>();
        sut.StartAutomation();
        sut.Dispose();

        // Assert
        rolledBackDisposable.DisposeCount.Should().Be(1);
        activeDisposable.DisposeCount.Should().Be(1);
    }

    private static IEnumerable<IDisposable> FailingSequence(DisposableSpy disposable)
    {
        yield return disposable;
        throw new InvalidOperationException("boom");
    }

    private sealed class ScriptedAutomation(
        ILogger logger,
        IReadOnlyList<Func<IEnumerable<IDisposable>>> scripts
    ) : AutomationBase(logger)
    {
        private int _index;

        protected override IEnumerable<IDisposable> GetAutomations() => scripts[_index++]();
    }

    private sealed class DisposableSpy : IDisposable
    {
        public int DisposeCount { get; private set; }

        public void Dispose() => DisposeCount++;
    }
}
