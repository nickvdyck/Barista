using System;
using System.IO;

namespace Barista.Common.FileSystem
{
    internal class LocalFileInfo : IFileInfo
    {
        private readonly FileInfo _info;

        public LocalFileInfo(FileInfo info)
        {
            _info = info;
        }

        public bool Exists => _info.Exists;

        public bool IsDirectory => false;

        public DateTimeOffset LastModified => _info.LastWriteTimeUtc;

        public long Length => _info.Length;

        public string Name => _info.Name;

        public string PhysicalPath => _info.FullName;
    }
}
