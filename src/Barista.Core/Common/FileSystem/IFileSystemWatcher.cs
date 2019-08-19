using System;

namespace Barista.Common.FileSystem
{
    public interface IFileSystemWatcher
    {
        event EventHandler Events;
    }
}
