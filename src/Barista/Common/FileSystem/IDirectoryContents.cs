using System.Collections.Generic;

namespace Barista.Common.FileSystem
{
    public interface IDirectoryContents : IEnumerable<IFileInfo>
    {
        bool Exists { get; }
    }
}
