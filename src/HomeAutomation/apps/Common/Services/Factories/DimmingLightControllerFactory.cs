namespace HomeAutomation.apps.Common.Services.Factories;

public interface IDimmingLightControllerFactory
{
    public IDimmingLightController Create(NumberEntity sensorDelay);
}

public class DimmingLightControllerFactory(ILogger<DimmingLightController> logger)
    : IDimmingLightControllerFactory
{
    public IDimmingLightController Create(NumberEntity sensorDelay) =>
        new DimmingLightController(sensorDelay, logger);
}
