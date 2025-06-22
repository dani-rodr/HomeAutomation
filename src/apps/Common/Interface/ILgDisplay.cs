namespace HomeAutomation.apps.Common.Interface;

public interface ILgDisplay : IMediaPlayer, IAutomationDevice
{
    void ShowPC();
    void ShowLaptop();
    void ShowScreenSaver();
    bool IsShowingPc { get; }
    bool IsShowingLaptop { get; }
}
