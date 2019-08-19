using System.Threading;
using System.Threading.Tasks;

namespace Barista.Common
{
    /// <summary>
    /// Mediator interface that encapsulates command , query and event interaction patterns
    /// </summary>
    public interface IMediator
    {
        Task Publish(IEvent @event, CancellationToken cancellationToken = default);

        Task Send(ICommand command, CancellationToken cancellationToken = default);

        Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
    }
}
