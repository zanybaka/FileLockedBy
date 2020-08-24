using System;
using System.Security.Permissions;
using FileLockedBy.Win32.System;
using Microsoft.Win32.SafeHandles;

namespace FileLockedBy.Win32.Entities
{
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal sealed class SafeObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeObjectHandle()
            : base(true)
        {
        }

        internal SafeObjectHandle(IntPtr preexistingHandle, bool ownsHandle)
            : base(ownsHandle)
        {
            base.SetHandle(preexistingHandle);
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(base.handle);
        }
    }
}