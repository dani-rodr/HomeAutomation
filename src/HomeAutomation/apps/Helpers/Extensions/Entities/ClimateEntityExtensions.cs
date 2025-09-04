using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class ClimateEntityExtensions
{
    public static bool IsDry([NotNullWhen(true)] this ClimateEntity? climate) =>
        climate.StateInvariant() is HaEntityStates.DRY;

    public static bool IsCool([NotNullWhen(true)] this ClimateEntity? climate) =>
        climate.StateInvariant() is HaEntityStates.COOL;

    public static bool IsOn([NotNullWhen(true)] this ClimateEntity? climate) =>
        climate.StateInvariant() is HaEntityStates.DRY or HaEntityStates.COOL;
}

public static class WeatherEntityExtensions
{
    public static bool IsDry([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather.StateInvariant() is HaEntityStates.DRY;

    public static bool IsSunny([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather.StateInvariant() is HaEntityStates.SUNNY or HaEntityStates.PARTLY_CLOUDY;

    public static bool IsRainy([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather.StateInvariant()
            is HaEntityStates.RAINY
                or HaEntityStates.POURING
                or HaEntityStates.LIGHTNING_RAINY;

    public static bool IsCloudy([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather.StateInvariant() is HaEntityStates.CLOUDY or HaEntityStates.PARTLY_CLOUDY;

    public static bool IsClearNight([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather.StateInvariant() is HaEntityStates.CLEAR_NIGHT;

    public static bool IsStormy([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather.StateInvariant()
            is HaEntityStates.LIGHTNING
                or HaEntityStates.LIGHTNING_RAINY
                or HaEntityStates.HAIL;

    public static bool IsSnowy([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather.StateInvariant() is HaEntityStates.SNOWY or HaEntityStates.SNOWY_RAINY;
}
