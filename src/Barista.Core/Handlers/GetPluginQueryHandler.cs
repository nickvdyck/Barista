using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Barista.Common;
using Barista.Data;
using Barista.Data.Queries;

namespace Barista.Handlers
{
    internal class GetPluginQueryHandler : IQueryHandler<GetPluginQuery, Plugin>
    {
        private readonly IPluginStore _store;
        public GetPluginQueryHandler(IPluginStore store)
        {
            _store = store;
        }

        public Task<Plugin> Handle(GetPluginQuery query, CancellationToken cancellationToken = default)
        {
            var plugin = _store.Plugins.FirstOrDefault(p => p.Name == query.Name);
            return Task.FromResult(plugin);
        }
    }
}
