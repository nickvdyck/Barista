using System.Collections.Generic;
using Barista.Core.Data;

namespace Barista.Core.Internal
{
    public interface IPluginOutputParser
    {
        IReadOnlyCollection<IPluginMenuItem> Parse(string output);
    }
}
