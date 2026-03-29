using HomeAutomation.apps.Common.Services.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.Tests.Common.Services.Logging;

public sealed class AutomationLogPolicyTests : HaContextTestBase
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AutomationLogPolicy _policy;
    private readonly string _logLevelEntityId;

    public AutomationLogPolicyTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHaContext>(HaContext);
        services.AddHomeAssistantGenerated();
        _serviceProvider = services.BuildServiceProvider();

        var inputSelectEntities = new InputSelectEntities(HaContext);
        var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _policy = new AutomationLogPolicy(scopeFactory);
        _logLevelEntityId = inputSelectEntities.AutomationLogLevel.EntityId;
    }

    [Fact]
    public void IsEnabled_Should_DefaultToDebug_WhenSelectorStateMissing()
    {
        _policy.IsEnabled(LogLevel.Trace).Should().BeFalse();
        _policy.IsEnabled(LogLevel.Debug).Should().BeTrue();
    }

    [Fact]
    public void IsEnabled_Should_RespectWarningSelector()
    {
        HaContext.SetEntityState(_logLevelEntityId, "Warning");

        _policy.IsEnabled(LogLevel.Information).Should().BeFalse();
        _policy.IsEnabled(LogLevel.Warning).Should().BeTrue();
    }

    [Fact]
    public void IsEnabled_Should_DisableAllLogs_WhenSelectorIsNone()
    {
        HaContext.SetEntityState(_logLevelEntityId, "None");

        _policy.IsEnabled(LogLevel.Critical).Should().BeFalse();
    }

    [Fact]
    public void ShouldWriteToLogbook_Should_WriteInformationForCuratedCategories()
    {
        var shouldWrite = _policy.ShouldWriteToLogbook(
            "HomeAutomation.apps.Security.Automations.AccessControlAutomation",
            LogLevel.Information
        );

        shouldWrite.Should().BeTrue();
    }

    [Fact]
    public void ShouldWriteToLogbook_Should_NotWriteInformationForNonCuratedCategories()
    {
        var shouldWrite = _policy.ShouldWriteToLogbook(
            "HomeAutomation.apps.Area.Bathroom.Automations.LightAutomation",
            LogLevel.Information
        );

        shouldWrite.Should().BeFalse();
    }

    [Fact]
    public void ShouldWriteToLogbook_Should_WriteWarningsForAppCategories()
    {
        var shouldWrite = _policy.ShouldWriteToLogbook(
            "HomeAutomation.apps.Common.Services.PersonController",
            LogLevel.Warning
        );

        shouldWrite.Should().BeTrue();
    }

    [Fact]
    public void ShouldWriteToLogbook_Should_NotWriteFrameworkWarnings()
    {
        var shouldWrite = _policy.ShouldWriteToLogbook(
            "Microsoft.Extensions.Hosting.Internal.Host",
            LogLevel.Warning
        );

        shouldWrite.Should().BeFalse();
    }
}
