using HomeAutomation.apps.Common.Services.Logging;

namespace HomeAutomation.Tests.Common.Services.Logging;

public sealed class LogbookSinkTests : HaContextTestBase
{
    [Fact]
    public void TryWrite_Should_CallLogbookLogService()
    {
        var sink = new LogbookSink(new HomeAssistantGenerated.Services(HaContext));

        sink.TryWrite(
            "HomeAutomation.apps.Area.Kitchen.Automations.CookingAutomation",
            LogLevel.Warning,
            "Cooktop power spike detected",
            null
        );

        HaContext.ShouldHaveCalledService("logbook", "log");
    }

    [Fact]
    public void TryWrite_Should_IncludeExceptionDetails_WhenExceptionProvided()
    {
        var sink = new LogbookSink(new HomeAssistantGenerated.Services(HaContext));

        sink.TryWrite(
            "HomeAutomation.apps.Area.Kitchen.Automations.CookingAutomation",
            LogLevel.Error,
            "Failed to process cooking state",
            new InvalidOperationException("boom")
        );

        var call = HaContext
            .ServiceCalls.Should()
            .ContainSingle(c => c.Domain == "logbook" && c.Service == "log")
            .Subject;

        var data = call.Data.Should().BeOfType<LogbookLogParameters>().Subject;
        data.Message.Should().Contain("Exception: InvalidOperationException - boom");
    }
}
