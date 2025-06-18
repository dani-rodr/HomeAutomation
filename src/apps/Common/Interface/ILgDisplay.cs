namespace HomeAutomation.apps.Common.Interface;

public interface ILgDisplay : IMediaPlayer
{
    void ShowPC();
    void ShowLaptop();
    void ShowScreenSaver();
    bool IsShowingPc { get; }
    bool IsShowingLaptop { get; }
}
