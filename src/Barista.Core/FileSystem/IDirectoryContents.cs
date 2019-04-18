using System.Collections.Generic;

namespace Barista.Core.FileSystem
{
    public interface IDirectoryContents : IEnumerable<IFileInfo>
    {
        bool Exists { get; }
    }
}
