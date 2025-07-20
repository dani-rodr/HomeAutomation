namespace HomeAutomation.apps.Common.Services.Factories;

public interface IDimmingLightControllerFactory
{
    public IDimmingLightController Create(NumberEntity sensorDelay);
}

public class DimmingLightControllerFactory(IScheduler scheduler, ILoggerFactory loggerFactory)
    : IDimmingLightControllerFactory
{
    public IDimmingLightController Create(NumberEntity sensorDelay) =>
        new DimmingLightController(
            sensorDelay,
            scheduler,
            loggerFactory.CreateLogger<DimmingLightController>()
        );
}
