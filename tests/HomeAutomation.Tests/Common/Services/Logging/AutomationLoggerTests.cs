using HomeAutomation.apps.Common.Services.Logging;

namespace HomeAutomation.Tests.Common.Services.Logging;

public sealed class AutomationLoggerTests
{
    private readonly Mock<ILoggerFactory> _loggerFactory = new();
    private readonly Mock<ILogger> _innerLogger = new();
    private readonly Mock<IAutomationLogPolicy> _policy = new();
    private readonly Mock<ILogbookSink> _logbookSink = new();

    public AutomationLoggerTests()
    {
        _loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_innerLogger.Object);

        _innerLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _policy.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    }

    [Fact]
    public void IsEnabled_Should_ReturnFalse_WhenPolicyDisablesLevel()
    {
        _policy.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(false);
        var logger = CreateLogger<SampleAutomation>();

        var enabled = logger.IsEnabled(LogLevel.Debug);

        enabled.Should().BeFalse();
    }

    [Fact]
    public void Log_Should_ForwardToInnerLogger_WhenEnabled()
    {
        var logger = CreateLogger<SampleAutomation>();

        logger.Log(
            LogLevel.Warning,
            new EventId(7, "evt"),
            "test-state",
            null,
            static (state, _) => $"formatted-{state}"
        );

        _innerLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.Is<EventId>(eventId => eventId.Id == 7),
                    It.Is<It.IsAnyType>((v, _) => v.ToString() == "test-state"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public void Log_Should_WriteToLogbook_WhenPolicyRequiresIt()
    {
        _policy
            .Setup(x =>
                x.ShouldWriteToLogbook(typeof(SampleAutomation).FullName!, LogLevel.Information)
            )
            .Returns(true);
        var logger = CreateLogger<SampleAutomation>();

        logger.Log(LogLevel.Information, default, "state", null, static (_, _) => "important info");

        _logbookSink.Verify(
            x =>
                x.TryWrite(
                    typeof(SampleAutomation).FullName!,
                    LogLevel.Information,
                    "important info",
                    null
                ),
            Times.Once
        );
    }

    [Fact]
    public void Log_Should_NotWriteToLogbook_WhenPolicyDisablesIt()
    {
        _policy
            .Setup(x => x.ShouldWriteToLogbook(It.IsAny<string>(), It.IsAny<LogLevel>()))
            .Returns(false);
        var logger = CreateLogger<SampleAutomation>();

        logger.Log(LogLevel.Information, default, "state", null, static (_, _) => "regular info");

        _logbookSink.Verify(
            x => x.TryWrite(It.IsAny<string>(), It.IsAny<LogLevel>(), It.IsAny<string>(), null),
            Times.Never
        );
    }

    [Fact]
    public void Log_Should_NotForward_WhenLevelDisabled()
    {
        _policy.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(false);
        _policy
            .Setup(x => x.ShouldWriteToLogbook(It.IsAny<string>(), It.IsAny<LogLevel>()))
            .Returns(false);
        var logger = CreateLogger<SampleAutomation>();

        logger.Log(LogLevel.Information, default, "state", null, static (_, _) => "ignored");

        _innerLogger.Verify(
            x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    [Fact]
    public void Log_Should_WriteToLogbook_WhenLevelDisabledButEntryIsImportant()
    {
        _policy.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(false);
        _policy
            .Setup(x =>
                x.ShouldWriteToLogbook(typeof(SampleAutomation).FullName!, LogLevel.Information)
            )
            .Returns(true);
        var logger = CreateLogger<SampleAutomation>();

        logger.Log(LogLevel.Information, default, "state", null, static (_, _) => "important info");

        _innerLogger.Verify(
            x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );

        _logbookSink.Verify(
            x =>
                x.TryWrite(
                    typeof(SampleAutomation).FullName!,
                    LogLevel.Information,
                    "important info",
                    null
                ),
            Times.Once
        );
    }

    private AutomationLogger<T> CreateLogger<T>() =>
        new(_loggerFactory.Object, _policy.Object, _logbookSink.Object);

    private sealed class SampleAutomation;
}
