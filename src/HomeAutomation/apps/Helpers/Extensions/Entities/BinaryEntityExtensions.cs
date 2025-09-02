using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class BinaryEntityExtensions
{
    public static IObservable<StateChange> OnOccupied(
        this BinarySensorEntity entity,
        DurationOptions? options = null
    ) => entity.OnTurnedOn(options);

    public static IObservable<StateChange> OnCleared(
        this BinarySensorEntity entity,
        DurationOptions? options = null
    ) => entity.OnTurnedOff(options);

    public static IObservable<StateChange> OnOpened(
        this BinarySensorEntity entity,
        DurationOptions? options = null
    ) => entity.OnTurnedOn(options);

    public static IObservable<StateChange> OnClosed(
        this BinarySensorEntity entity,
        DurationOptions? options = null
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
