using System.Reactive.Disposables;

namespace HomeAutomation.apps.Area.LivingRoom.Devices;

public class TclDisplay(ITclDisplayEntities entities, ILogger logger)
    : MediaPlayerBase(entities.MediaPlayer, logger),
        ITclDisplay
{
    protected override Dictionary<string, string> ExtendedSources => [];
}
