using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public record BinaryDuration(
    bool StartImmediately = true,
    bool AllowFromUnavailable = true,
    int Days = 0,
    int Hours = 0,
    int Minutes = 0,
    int Seconds = 0,
    int Milliseconds = 0,
    Func<EntityState<BinarySensorAttributes>?, bool>? Condition = null
)
    : DurationOptions<EntityState<BinarySensorAttributes>>(
        StartImmediately,
        AllowFromUnavailable,
        Days,
        Hours,
        Minutes,
        Seconds,
        Milliseconds,
        Condition
    );

public static class BinaryEntityExtensions
{
    public static IObservable<
        StateChange<BinarySensorEntity, EntityState<BinarySensorAttributes>>
    > OnOccupied(
        this BinarySensorEntity entity,
        DurationOptions<EntityState<BinarySensorAttributes>>? options = null
    ) => entity.OnTurnedOn(options);

    public static IObservable<
        StateChange<BinarySensorEntity, EntityState<BinarySensorAttributes>>
    > OnOpened(
        this BinarySensorEntity entity,
        DurationOptions<EntityState<BinarySensorAttributes>>? options = null
    ) => entity.OnTurnedOn(options);

    public static IObservable<
        StateChange<BinarySensorEntity, EntityState<BinarySensorAttributes>>
    > OnCleared(
        this BinarySensorEntity entity,
        DurationOptions<EntityState<BinarySensorAttributes>>? options = null
    ) => entity.OnTurnedOff(options);

    public static IObservable<
        StateChange<BinarySensorEntity, EntityState<BinarySensorAttributes>>
    > OnClosed(
        this BinarySensorEntity entity,
        DurationOptions<EntityState<BinarySensorAttributes>>? options = null
    ) => entity.OnTurnedOff(options);

    public static bool IsOpen([NotNullWhen(true)] this BinarySensorEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.ON;

    public static bool IsClosed([NotNullWhen(true)] this BinarySensorEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.CLOSED;

    public static bool IsOccupied([NotNullWhen(true)] this BinarySensorEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.ON;

    public static bool IsClear([NotNullWhen(true)] this BinarySensorEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.OFF;
}
