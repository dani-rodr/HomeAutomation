namespace HomeAutomation.apps.Common.Interface;

public interface ILgDisplay : IMediaPlayer, IAutomation
{
    void ShowPC();
    void ShowLaptop();
    void ShowScreenSaver();
    void ShowToast(string message, params object[] args);
    bool IsShowingPc { get; }
    bool IsShowingLaptop { get; }
    public IObservable<bool> ScreenChanges { get; }
    Task SetBrightnessAsync(int value);
    Task SetBrightnessHighAsync();
    Task SetBrightnessLowAsync();
}
