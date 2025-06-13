namespace HomeAutomation.apps.Common.Containers;

public interface ILgDisplayEntities
{
    MediaPlayerEntity LgWebosSmartTv { get; }
}

public class DeskLgDisplayEntities(Entities entities) : ILgDisplayEntities
{
    public MediaPlayerEntity LgWebosSmartTv => entities.MediaPlayer.LgWebosSmartTv;
}
