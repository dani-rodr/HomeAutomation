using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class PersonEntityExtensions
{
    public static bool IsHome([NotNullWhen(true)] this PersonEntity entity) =>
        entity.StateInvariant() is HaEntityStates.HOME;

    public static bool IsAway([NotNullWhen(true)] this PersonEntity entity) =>
        entity.StateInvariant() is HaEntityStates.AWAY;
}
