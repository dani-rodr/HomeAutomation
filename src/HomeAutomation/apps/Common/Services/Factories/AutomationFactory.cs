using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Common.Services.Factories;

public class AutomationFactory(IServiceProvider serviceProvider) : IAutomationFactory
{
    public TAutomation Create<TAutomation>(params object[] arguments)
        where TAutomation : class, IAutomation
    {
        return ActivatorUtilities.CreateInstance<TAutomation>(serviceProvider, arguments);
    }
}
