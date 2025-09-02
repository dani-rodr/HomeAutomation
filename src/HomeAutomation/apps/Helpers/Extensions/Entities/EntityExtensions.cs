using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public record DurationOptions(
    bool ShouldCheckImmediately = false,
    bool ShouldCheckIfAutomated = false,
    bool ShouldCheckIfPhysicallyOperated = false,
    bool ShouldCheckIfManuallyOperated = false,
    int Days = 0,
    int Hours = 0,
    int Minutes = 0,
    int Seconds = 0,
    int Milliseconds = 0
)
{
    public TimeSpan TimeSpan => new(Days, Hours, Minutes, Seconds, Milliseconds);
}

public static class EntityExtensions
{
    public static bool IsUnavailable([NotNullWhen(true)] this Entity? entity) =>
        entity?.State is HaEntityStates.UNAVAILABLE;

    public static bool IsUnknown([NotNullWhen(true)] this Entity? entity) =>
        entity?.State is HaEntityStates.UNAVAILABLE;

    private static IObservable<StateChange> GetStateChange(
        this Entity entity,
        bool shouldCheckImmediately
    ) => shouldCheckImmediately ? entity.StateChangesWithCurrent() : entity.StateChanges();

    private static IObservable<StateChange> FilterByIdentity(
        this IObservable<StateChange> stream,
        DurationOptions options
    ) =>
        options switch
        {
            { ShouldCheckIfAutomated: true } => stream.Where(s =>
                HaIdentity.IsAutomated(s.UserId())
            ),
            { ShouldCheckIfPhysicallyOperated: true } => stream.Where(s =>
                HaIdentity.IsPhysicallyOperated(s.UserId())
            ),
            { ShouldCheckIfManuallyOperated: true } => stream.Where(s =>
                HaIdentity.IsManuallyOperated(s.UserId())
            ),
            _ => stream,
        };

    private static IObservable<StateChange> WhenIsFor(
        this IObservable<StateChange> source,
        Func<EntityState?, bool> predicate,
        TimeSpan duration
    ) =>
        duration > TimeSpan.Zero
            ? source.WhenStateIsFor(predicate, duration, SchedulerProvider.Current)
            : source.Where(sc => predicate(sc.New));

    public static IObservable<StateChange> OnChanges(
        this Entity entity,
        Func<EntityState?, bool>? predicate = null,
        DurationOptions? options = null
    )
    {
        options ??= new DurationOptions();
        predicate ??= _ => true;

        return entity
            .GetStateChange(options.ShouldCheckImmediately)
            .WhenIsFor(predicate, options.TimeSpan)
            .FilterByIdentity(options);
    }

    public static IObservable<StateChange> OnTurnedOn(
        this Entity entity,
        DurationOptions? options = null
    ) => entity.OnChanges(s => s.IsOn(), options);

    public static IObservable<StateChange> OnTurnedOff(
        this Entity entity,
        DurationOptions? options = null
    ) => entity.OnChanges(s => s.IsOff(), options);
}

