namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class ButtonEntityExtensions
{
    public static IObservable<StateChange> OnPressed(this ButtonEntity entity) =>
        entity.StateChanges().Where(e => DateTime.TryParse(e.New?.State, out _));
}

public static class InputButtonEntityExtensions
{
    public static IObservable<StateChange> OnPressed(this InputButtonEntity entity) =>
        entity.StateChanges().Where(e => DateTime.TryParse(e.New?.State, out _));
}
