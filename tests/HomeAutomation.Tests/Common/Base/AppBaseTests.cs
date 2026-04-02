using HomeAutomation.apps.Common.Base;
using HomeAutomation.apps.Common.Interface;
using NetDaemon.AppModel;

namespace HomeAutomation.Tests.Common.Base;

public class AppBaseTests
{
    [Fact]
    public void Constructor_WhenStartAutomationFailsMidSequence_ShouldDisposePreviouslyStartedAutomations()
    {
        // Arrange
        var first = new Mock<IAutomation>();
        var second = new Mock<IAutomation>();
        var third = new Mock<IAutomation>();

        second
            .Setup(x => x.StartAutomation())
            .Throws(new InvalidOperationException("start failed"));

        // Act
        var act = () =>
            _ = new TestApp(
                new TestAppConfig<NoAppSettings>(new()),
                [first.Object, second.Object, third.Object]
            );

        // Assert
        act.Should().Throw<InvalidOperationException>();
        first.Verify(x => x.StartAutomation(), Times.Once);
        second.Verify(x => x.StartAutomation(), Times.Once);
        third.Verify(x => x.StartAutomation(), Times.Never);
        first.Verify(x => x.Dispose(), Times.Once);
        second.Verify(x => x.Dispose(), Times.Once);
        third.Verify(x => x.Dispose(), Times.Never);
    }

    [Fact]
    public void Dispose_WhenAllAutomationsStarted_ShouldDisposeAll()
    {
        // Arrange
        var first = new Mock<IAutomation>();
        var second = new Mock<IAutomation>();
        var app = new TestApp(
            new TestAppConfig<NoAppSettings>(new()),
            [first.Object, second.Object]
        );

        // Act
        app.Dispose();

        // Assert
        first.Verify(x => x.StartAutomation(), Times.Once);
        second.Verify(x => x.StartAutomation(), Times.Once);
        first.Verify(x => x.Dispose(), Times.Once);
        second.Verify(x => x.Dispose(), Times.Once);
    }

    private sealed class TestApp(
        IAppConfig<NoAppSettings> appConfig,
        IReadOnlyList<IAutomation> automations
    ) : AppBase<NoAppSettings>(appConfig)
    {
        protected override IEnumerable<IAutomation> CreateAutomations() => automations;
    }

    private sealed class TestAppConfig<T>(T value) : IAppConfig<T>
        where T : class, new()
    {
        public T Value { get; } = value;
    }
}
