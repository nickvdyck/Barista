using System;
using System.IO;

namespace Barista.Common.FileSystem
{
    internal class LocalDirectoryInfo : IFileInfo
    {
        private readonly DirectoryInfo _info;

        public LocalDirectoryInfo(DirectoryInfo info)
        {
            _info = info;
        }

        public bool Exists => _info.Exists;
        public bool IsDirectory => true;
        public DateTimeOffset LastModified => _info.LastWriteTimeUtc;
        public long Length => -1;
        public string Name => _info.Name;
        public string PhysicalPath => _info.FullName;
    }
}
