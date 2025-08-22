namespace HomeAutomation.apps.Area.LivingRoom.Devices;

public class TclDisplay(ITclDisplayEntities entities, ILogger<TclDisplay> logger)
    : MediaPlayerBase(entities.MediaPlayer, logger),
        ITclDisplay
{
    protected override Dictionary<string, string> ExtendedSources => [];
}
