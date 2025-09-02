using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class LockEntityExtensions
{
    public static bool IsLocked([NotNullWhen(true)] this LockEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.LOCKED;

    public static bool IsUnlocked([NotNullWhen(true)] this LockEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.UNLOCKED;
}
