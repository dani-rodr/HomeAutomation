namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class LightEntityExtensions
{
    public static void TurnOnWithBrightness(this LightEntity entity, int brightness) =>
        entity.TurnOn(brightnessPct: brightness);
}
