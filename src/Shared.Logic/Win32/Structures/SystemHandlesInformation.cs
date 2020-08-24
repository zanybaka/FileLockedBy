using System.Runtime.InteropServices;

namespace FileLockedBy.Win32.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemHandlesInformation
    {
        public int Count;
        public SystemHandleEntry FirstHandle;
    }
}