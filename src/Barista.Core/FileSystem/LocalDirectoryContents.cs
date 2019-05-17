using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Barista.Core.FileSystem
{
    public class LocalDirectoryContents : IDirectoryContents
    {
        private IEnumerable<IFileInfo> _entries;
        private readonly string _directory;

        public LocalDirectoryContents(string directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        public bool Exists => Directory.Exists(_directory);

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            EnumerateDirectoryContents();
            return _entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            EnumerateDirectoryContents();
            return _entries.GetEnumerator();
        }

        private void EnumerateDirectoryContents()
        {
            try
            {
                _entries = new DirectoryInfo(_directory)
                    .EnumerateFileSystemInfos()
                    .Select<FileSystemInfo, IFileInfo>(info =>
                    {
                        if (info is FileInfo file)
                        {
                            return new LocalFileInfo(file);
                        }
                        else if (info is DirectoryInfo dir)
                        {
                            return new LocalDirectoryInfo(dir);
                        }
                        // shouldn't happen unless BCL introduces new implementation of base type
                        throw new InvalidOperationException("Unexpected type of FileSystemInfo");
                    });
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is IOException)
            {
                _entries = Enumerable.Empty<IFileInfo>();
            }
        }
    }
}
