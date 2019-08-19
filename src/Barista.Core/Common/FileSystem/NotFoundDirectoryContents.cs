using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Barista.Common.FileSystem
{
    public class NotFoundDirectoryContents : IDirectoryContents
    {
        public static NotFoundDirectoryContents Singleton { get; } = new NotFoundDirectoryContents();

        public bool Exists => false;

        public IEnumerator<IFileInfo> GetEnumerator() => Enumerable.Empty<IFileInfo>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
