using System;
using MediatR;

namespace Barista.Common.Redux
{
    public interface IReduxStore<TState> : IDisposable where TState : class, new()
    {
        IObservable<TState> Select();

        IObservable<TSelectorResult> Select<TSelectorResult>(Func<TState, TSelectorResult> selector);

        void Dispatch(IAction action);

        void Dispatch(INotification effect);

        void Reset();
    }
}
