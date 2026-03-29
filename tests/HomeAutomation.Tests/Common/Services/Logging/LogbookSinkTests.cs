using HomeAutomation.apps.Common.Services.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.Tests.Common.Services.Logging;

public sealed class LogbookSinkTests : HaContextTestBase
{
    private readonly ServiceProvider _serviceProvider;

    public LogbookSinkTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHaContext>(HaContext);
        services.AddHomeAssistantGenerated();
        services.AddTransient<IServices, HomeAssistantGenerated.Services>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void TryWrite_Should_CallLogbookLogService()
    {
        var sink = CreateSink();

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
        var sink = CreateSink();

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

    [Fact]
    public void TryWrite_Should_DeduplicateRepeatedInformationLogs_WithinWindow()
    {
        var sink = CreateSink();

        sink.TryWrite(
            "HomeAutomation.apps.Security.Automations.AccessControlAutomation",
            LogLevel.Information,
            "Door unlocked for Daniel",
            null
        );

        sink.TryWrite(
            "HomeAutomation.apps.Security.Automations.AccessControlAutomation",
            LogLevel.Information,
            "Door unlocked for Daniel",
            null
        );

        HaContext
            .ServiceCalls.Count(c => c.Domain == "logbook" && c.Service == "log")
            .Should()
            .Be(1);
    }

    private LogbookSink CreateSink() =>
        new(_serviceProvider.GetRequiredService<IServiceScopeFactory>());
}
