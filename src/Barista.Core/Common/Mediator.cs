using Barista.Common.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Barista.Common
{
    internal class Mediator : IMediator
    {
        readonly ServiceProvider _provider;

        private static readonly ConcurrentDictionary<Type, EventHandlerInvoker> _eventHandlers = new ConcurrentDictionary<Type, EventHandlerInvoker>();
        private static readonly ConcurrentDictionary<Type, object> _cqrsHandlers = new ConcurrentDictionary<Type, object>();

        public Mediator(ServiceProvider provider)
        {
            _provider = provider;
        }

        internal abstract class EventHandlerInvoker
        {
            public abstract Task Handle(IEvent @event, CancellationToken token, ServiceProvider provider);

        }

        internal class EventHandlerInvoker<TEvent> : EventHandlerInvoker where TEvent : IEvent
        {
            public override async Task Handle(IEvent @event, CancellationToken token, ServiceProvider provider)
            {
                var handlers = provider
                    .GetInstances<IEventHandler<TEvent>>();

                foreach (var handler in handlers)
                {
                    try
                    {
                        await handler.Handle((TEvent)@event, token);
                    }
                    catch (Exception ex)
                    {
                        // TODO: Improve error handling
                        Debug.WriteLine("Noo.");
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        public Task Publish(IEvent @event, CancellationToken token = default)
        {
            var type = @event.GetType();
            var handler = _eventHandlers
                .GetOrAdd(
                    type,
                    t => (EventHandlerInvoker)Activator.CreateInstance(typeof(EventHandlerInvoker<>).MakeGenericType(type))
                );

            return handler.Handle(@event, token, _provider);
        }

        internal abstract class CommandHandlerInvoker
        {
            public abstract Task Handle(ICommand command, CancellationToken token, ServiceProvider provider);
        }

        internal class CommandHandlerInvoker<TCommand> : CommandHandlerInvoker where TCommand : ICommand
        {
            public override Task Handle(ICommand command, CancellationToken token, ServiceProvider provider) =>
                provider
                    .GetInstance<ICommandHandler<TCommand>>()
                    .Handle((TCommand)command, token);
        }

        public Task Send(ICommand command, CancellationToken token = default)
        {
            var type = command.GetType();
            var handler = (CommandHandlerInvoker)_cqrsHandlers
                .GetOrAdd(
                    type,
                    t => Activator.CreateInstance(typeof(CommandHandlerInvoker<>).MakeGenericType(type))
                );

            return handler.Handle(command, token, _provider);
        }

        internal abstract class QueryHandlerInvoker<TResponse>
        {
            public abstract Task<TResponse> Handle(IQuery<TResponse> query, CancellationToken token, ServiceProvider provider);
        }

        internal class QueryHandlerInvoker<TQuery, TResponse> : QueryHandlerInvoker<TResponse> where TQuery : IQuery<TResponse>
        {
            public override Task<TResponse> Handle(IQuery<TResponse> query, CancellationToken token, ServiceProvider provider) =>
                provider
                    .GetInstance<IQueryHandler<TQuery, TResponse>>()
                    .Handle((TQuery)query, token);
        }

        public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken token = default)
        {
            var type = query.GetType();
            var handler = (QueryHandlerInvoker<TResponse>)_cqrsHandlers
                .GetOrAdd(
                    type,
                    t => Activator.CreateInstance(typeof(QueryHandlerInvoker<,>).MakeGenericType(type, typeof(TResponse)))
                );

            return handler.Handle(query, token, _provider);
        }
    }
}
