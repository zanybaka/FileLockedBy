using System;
using System.IO;

namespace FileLockedBy.Win32.Structures
{
    public struct FileHandleInfo
    {
        public readonly FileSystemInfo FileSystemInfo;
        public readonly IntPtr Handle;

        public FileHandleInfo(IntPtr handle, FileSystemInfo fileSystemInfo)
        {
            Handle = handle;
            FileSystemInfo = fileSystemInfo;
        }
    }
}