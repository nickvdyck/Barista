namespace Barista.Core.FileSystem
{
    public interface IFileProvider
    {
        IDirectoryContents GetDirectoryContents(string path = "");
        IFileInfo GetFileInfo(string filePath);
    }
}