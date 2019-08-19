using Barista.Common;
using Barista.Data;

namespace Barista.Data.Queries
{
    public class GetPluginQuery : IQuery<Plugin>
    {
        public string Name { get; set; }
    }
}
