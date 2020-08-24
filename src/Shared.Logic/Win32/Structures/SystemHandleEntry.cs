using System;
using System.Runtime.InteropServices;

namespace FileLockedBy.Win32.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemHandleEntry
    {
        public int OwnerPid;
        public byte ObjectType;
        public byte HandleFlags;
        public short HandleValue;
        public IntPtr ObjectPointer;
        public int AccessMask;
    }
}