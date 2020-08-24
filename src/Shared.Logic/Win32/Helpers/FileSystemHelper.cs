using System.Collections.Generic;
using FileLockedBy.Win32.Entities;
using FileLockedBy.Win32.FileSystem;
using FileLockedBy.Win32.Structures;

namespace FileLockedBy.Win32.Helpers
{
    internal static class FileSystemHelper
    {
        public static IEnumerator<FileHandleInfo> GetOpenFilesEnumerator(int processId, SmartPtr systemHandleInformationPointer, int handleCount)
        {
            return FileSystemInformation.GetOpenFileHandleEnumerator(processId, systemHandleInformationPointer, handleCount);
        }
    }
}