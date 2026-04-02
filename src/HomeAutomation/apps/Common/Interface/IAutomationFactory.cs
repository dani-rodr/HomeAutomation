namespace HomeAutomation.apps.Common.Interface;

public interface IAutomationFactory
{
    TAutomation Create<TAutomation>(params object[] arguments)
        where TAutomation : class, IAutomation;
}
