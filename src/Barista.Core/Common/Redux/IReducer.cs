namespace Barista.Common.Redux
{
    public interface IReducer<TState>
        where TState : class, new()
        //where TAction : IAction
    {
        TState Execute(TState state, IAction action);
    }
}
