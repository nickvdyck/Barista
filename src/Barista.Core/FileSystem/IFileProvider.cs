using System;

namespace Barista.Core.FileSystem
{
    public interface IFileProvider
    {
        IDirectoryContents GetDirectoryContents(string path = "");
        IFileInfo GetFileInfo(string filePath);
        IDisposable Watch(Action onChange);
    }
}