using System;

namespace FileLockedBy.Win32.Enums
{
    [Flags]
    internal enum DuplicateHandleOptions
    {
        CloseSourceHandle = 0x1,
        SameAccess = 0x2
    }
}