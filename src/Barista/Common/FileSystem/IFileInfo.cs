using System;

namespace Barista.Common.FileSystem
{
    public interface IFileInfo
    {
        bool Exists { get; }
        bool IsDirectory { get; }
        DateTimeOffset LastModified { get; }
        long Length { get; }
        string Name { get; }
        string PhysicalPath { get; }
    }
}
