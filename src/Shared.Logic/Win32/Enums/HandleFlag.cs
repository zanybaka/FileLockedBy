using System;

namespace FileLockedBy.Win32.Enums
{
    [Flags]
    internal enum HandleFlag : uint 
    {
        None = 0x00000000,
        Inherit = 0x00000001,
        ProtectFromClose = 0x00000002
    }
}