namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class LightEntityExtensions
{
    public static double Brightness(this LightEntity entity) => entity?.Attributes?.Brightness ?? 0;
}
