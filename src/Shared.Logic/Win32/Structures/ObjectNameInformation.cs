using System.Runtime.InteropServices;

namespace FileLockedBy.Win32.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ObjectNameInformation
    {
        public UnicodeString Name;
    }
}