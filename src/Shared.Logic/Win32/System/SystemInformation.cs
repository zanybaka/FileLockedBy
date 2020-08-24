using System;
using System.Diagnostics;
using FileLockedBy.Win32.Entities;
using FileLockedBy.Win32.Enums;

namespace FileLockedBy.Win32.System
{
    public static class SystemInformation
    {
        #region Delegates

        public delegate bool HandleDelegate(IntPtr handle, params object[] args);

        #endregion

        public static bool HandleAction(IntPtr handle, int processId, HandleDelegate handleDelegate,
                                        params object[] args)
        {
            Process currentProcess = Process.GetCurrentProcess();

            bool remote = (processId != currentProcess.Id);

            SafeProcessHandle processHandle = null;

            SafeObjectHandle objectHandle = null;

            try
            {
                if (remote)
                {
                    processHandle = NativeMethods.OpenProcess(ProcessAccessRights.DuplicateHandle, true, processId);

                    if (NativeMethods.DuplicateHandle(processHandle.DangerousGetHandle(), handle, currentProcess.Handle,
                                                      out objectHandle, 0, false,
                                                      DuplicateHandleOptions.SameAccess))
                    {
                        handle = objectHandle.DangerousGetHandle();
                    }
                }
                return handleDelegate.Invoke(handle, args);
            }

            finally
            {
                if (remote)
                {
                    if (processHandle != null)
                    {
                        processHandle.Close();
                    }

                    if (objectHandle != null)
                    {
                        objectHandle.Close();
                    }
                }
            }
        }

        public static SmartPtr GetSystemHandleInformation()
        {
            int length = 0x10000;
            while (true)
            {
                using (SmartPtr ptr = new SmartPtr())
                {
                    ptr.Allocate(length);

                    int returnLength;
                    NtStatus ret = NativeMethods.NtQuerySystemInformation(
                        SystemInformationClass.
                            SystemHandleInformation,
                        ptr.Pointer, length, out returnLength
                        );
                    if (ret == NtStatus.InfoLengthMismatch)
                    {
                        length *= 2;
                        if (length > 1024*1024*1024)
                        {
                            throw new OutOfMemoryException("NtQuerySystemInformation fails");
                        }
                    }
                    else if (ret == NtStatus.Success)
                    {
                        return ptr.Clone();
                    }
                }
            }
        }
    }
}