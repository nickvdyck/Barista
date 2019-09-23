using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using DryIoc;
using MediatR;

namespace Barista.Common.Redux
{
    internal class ReduxStore<TState> : IReduxStore<TState> where TState : class, new()
    {
        private readonly IMediator _mediator;
        private readonly BehaviorSubject<TState> _stateSubject;
        private readonly FullStateComparer<TState> _fullStateComparer = new FullStateComparer<TState>();
        private readonly IEnumerable<IReducer<TState>> _reducers;

        private TState State { get; set; }

        public ReduxStore(IMediator mediator, IEnumerable<IReducer<TState>> reducers)
        {
            _mediator = mediator;
            State = Activator.CreateInstance<TState>();
            _stateSubject = new BehaviorSubject<TState>(State);
            _reducers = reducers;
        }

        public void Dispatch(IAction action)
        {
            foreach (var reducer in _reducers)
            {
                State = reducer.Execute(State, action);
            }

            _stateSubject.OnNext(State);
        }

        public void Dispatch(INotification notification)
        {
            _mediator.Publish(notification);
        }

        public IObservable<TState> Select()
        {
            return _stateSubject
                .DistinctUntilChanged(_fullStateComparer);
        }

        public IObservable<TSelectorResult> Select<TSelectorResult>(Func<TState, TSelectorResult> selector)
        {
            return _stateSubject
                .Select(selector)
                .DistinctUntilChanged();
        }

        public void Reset()
        {
            State = Activator.CreateInstance<TState>();
            _stateSubject.OnNext(State);
        }

        public void Dispose()
        {
            _stateSubject.Dispose();
        }
    }
}
