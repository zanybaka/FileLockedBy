using System;

namespace FileLockedBy.Win32.Enums
{
    [Flags]
    internal enum ProcessAccessRights
    {
        DuplicateHandle = 0x00000040
    }
}