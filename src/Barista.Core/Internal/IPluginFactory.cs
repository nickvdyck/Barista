using Barista.Core.Data;

namespace Barista.Core.Internal
{
    public interface IPluginFactory
    {
        Plugin FromFilePath(string path);
    }
}
