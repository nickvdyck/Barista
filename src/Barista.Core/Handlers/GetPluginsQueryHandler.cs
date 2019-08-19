using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Barista.Common;
using Barista.Data;
using Barista.Data.Queries;

namespace Barista.Handlers
{
    internal class GetPluginsQueryHandler : IQueryHandler<GetPluginsQuery, IReadOnlyCollection<Plugin>>
    {
        private readonly IPluginStore _store;
        public GetPluginsQueryHandler(IPluginStore store)
        {
            _store = store;
        }

        public Task<IReadOnlyCollection<Plugin>> Handle(GetPluginsQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Plugins);
        }
    }
}
