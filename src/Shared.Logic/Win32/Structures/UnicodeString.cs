using System;
using System.Runtime.InteropServices;

namespace FileLockedBy.Win32.Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct UnicodeString
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;

        public string GetValue()
        {
            if (Length == 0)
            {
                return string.Empty;
            }
            return Marshal.PtrToStringUni(Buffer, Length/2);
        }
    }
}