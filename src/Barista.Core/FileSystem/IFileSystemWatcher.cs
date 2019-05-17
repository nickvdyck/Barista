using System;

namespace Barista.Core.FileSystem
{
    public interface IFileSystemWatcher
    {
        event EventHandler Events;
    }
}