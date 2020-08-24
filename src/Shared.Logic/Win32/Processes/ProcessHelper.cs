using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using FileLockedBy.Enums;
using FileLockedBy.Win32.Entities;
using FileLockedBy.Win32.Enums;
using FileLockedBy.Win32.Helpers;
using FileLockedBy.Win32.Structures;
using FileLockedBy.Win32.System;

namespace FileLockedBy.Win32.Processes
{
    public static class ProcessHelper
    {
        private static readonly WindowsIdentity CurrentUser = WindowsIdentity.GetCurrent();

        public static IEnumerator<FileHandleInfo> GetCurrentProcessOpenFilesEnumerator(int processId, SmartPtr systemHandleInformationPointer, int handleCount)
        {
            return FileSystemHelper.GetOpenFilesEnumerator(processId, systemHandleInformationPointer, handleCount);
        }

        public static bool IsCurrentUserProcess(int processId)
        {
            ProcessInformation.ExGetProcessInfoByPID(processId, out string sid);
            return string.Compare(sid, CurrentUser.User.Value, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static Error CloseHandle(Process handleOwnerProcess, FileHandleInfo handle, Process currentProcess)
        {
            SafeProcessHandle remoteProcess = NativeMethods.OpenProcess(ProcessAccessRights.DuplicateHandle, true, handleOwnerProcess.Id);

            bool result = NativeMethods.DuplicateHandle(remoteProcess.DangerousGetHandle(), handle.Handle,
                currentProcess.Handle,
                out SafeObjectHandle duplicateHandle, 0, false,
                DuplicateHandleOptions.CloseSourceHandle);
            if (!result)
            {
                return Error.DuplicateHandleFails;
            }

            result = NativeMethods.GetHandleInformation(duplicateHandle.DangerousGetHandle(),
                out HandleFlag flag);
            if (!result)
            {
                return Error.GetHandleInformationFails;
            }

            if (flag == HandleFlag.ProtectFromClose)
            {
                return Error.HandleProtectedFromClosed;
            }

            result = NativeMethods.CloseHandle(duplicateHandle.DangerousGetHandle());
            if (!result)
            {
                int lastError = Marshal.GetLastWin32Error();
                return (Error)lastError;
            }

            return Error.None;
        }
    }
}