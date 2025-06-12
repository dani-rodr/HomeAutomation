namespace HomeAutomation.apps.Common.Interface;

public interface IMediaPlayer
{
    void TurnOn();
    void TurnOff();
    bool IsOn();
    bool IsOff();
}
