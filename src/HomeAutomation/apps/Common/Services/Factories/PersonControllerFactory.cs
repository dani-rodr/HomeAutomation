namespace HomeAutomation.apps.Common.Services.Factories;

public interface IPersonControllerFactory
{
    IPersonController Create(IPersonEntities entities);
}

public class PersonControllerFactory(IServices services, ILogger<PersonController> logger)
    : IPersonControllerFactory
{
    public IPersonController Create(IPersonEntities entities)
    {
        return new PersonController(entities, services, logger);
    }
}
