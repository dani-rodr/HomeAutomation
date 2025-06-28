namespace HomeAutomation.apps.Common.Interface;

public interface IAutomation : IDisposable
{
    void StartAutomation();
}

public interface IAutomationDevice : IAutomation;
