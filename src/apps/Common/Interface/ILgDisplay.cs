namespace HomeAutomation.apps.Common.Interface;

public interface ILgDisplay : IMediaPlayer, IAutomation
{
    void ShowPC();
    void ShowLaptop();
    void ShowScreenSaver();
    void ShowToast(string msg);
    bool IsShowingPc { get; }
    bool IsShowingLaptop { get; }
    Task SetBrightnessAsync(int value);
    Task SetBrightnessHighAsync();
    Task SetBrightnessLowAsync();
}
