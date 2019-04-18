using System.IO;

namespace Barista.Core.FileSystem
{
    public class LocalFileProvider : IFileProvider
    {
        private readonly string _root;

        public LocalFileProvider(string root)
        {
            _root = root;
        }
        public IDirectoryContents GetDirectoryContents(string path = "")
        {
            try
            {
                var fullPath = Path.Combine(_root, path);

                if (fullPath == null || !Directory.Exists(fullPath))
                {
                    return NotFoundDirectoryContents.Singleton;
                }

                return new LocalDirectoryContents(fullPath);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (IOException)
            {
            }

            return NotFoundDirectoryContents.Singleton;
        }

        public IFileInfo GetFileInfo(string filePath)
        {
            throw new System.NotImplementedException();
        }
    }
}