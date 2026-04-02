using HomeAutomation.apps.Common;

namespace HomeAutomation.Tests.Common;

public class StartupAppTests
{
    [Fact]
    public async Task DismissNotificationLaterAsync_WhenDismissThrows_ShouldSwallowAndLogError()
    {
        // Arrange
        var logger = new Mock<ILogger>();

        // Act
        var act = async () =>
            await StartupApp.DismissNotificationLaterAsync(
                () => throw new InvalidOperationException("dismiss failed"),
                logger.Object,
                TimeSpan.Zero
            );

        // Assert
        await act.Should().NotThrowAsync();
        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
