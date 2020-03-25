using System;

namespace Barista.Common.FileSystem
{
    public interface IFileProvider
    {
        IDirectoryContents GetDirectoryContents(string path = "");
        IFileInfo GetFileInfo(string filePath);
        IDisposable Watch(Action onChange);
    }
}
