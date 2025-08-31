namespace HomeAutomation.apps.Common.Services.Factories;

public interface IDimmingLightControllerFactory
{
    public IDimmingLightController Create(NumberEntity sensorDelay);
}

public class DimmingLightControllerFactory(ILoggerFactory loggerFactory)
    : IDimmingLightControllerFactory
{
    public IDimmingLightController Create(NumberEntity sensorDelay) =>
        new DimmingLightController(
            sensorDelay,
            loggerFactory.CreateLogger<DimmingLightController>()
        );
}
