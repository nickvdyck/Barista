using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Barista.Core.FileSystem
{
    public class NotFoundDirectoryContents : IDirectoryContents
    {
        public static NotFoundDirectoryContents Singleton { get; } = new NotFoundDirectoryContents();

        public bool Exists => false;

        public IEnumerator<IFileInfo> GetEnumerator() => Enumerable.Empty<IFileInfo>().GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
