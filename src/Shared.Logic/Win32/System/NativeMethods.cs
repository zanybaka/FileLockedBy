//
// Source: http://sadeveloper.net/forums/p/358/1441.aspx
//

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using FileLockedBy.Win32.Entities;
using FileLockedBy.Win32.Enums;

namespace FileLockedBy.Win32.System
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern bool GetHandleInformation(IntPtr hObject, out HandleFlag lpdwFlags);

        [DllImport("ntdll.dll")]
        internal static extern NtStatus NtQuerySystemInformation(
            [In] SystemInformationClass systemInformationClass,
            [In] IntPtr systemInformation,
            [In] int systemInformationLength,
            [Out] out int returnLength);

        [DllImport("ntdll.dll")]
        internal static extern NtStatus NtQueryObject(
            [In] IntPtr handle,
            [In] ObjectInformationClass objectInformationClass,
            [In] IntPtr objectInformation,
            [In] int objectInformationLength,
            [Out] out int returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeProcessHandle OpenProcess(
            [In] ProcessAccessRights dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DuplicateHandle(
            [In] IntPtr hSourceProcessHandle,
            [In] IntPtr hSourceHandle,
            [In] IntPtr hTargetProcessHandle,
            [Out] out SafeObjectHandle lpTargetHandle,
            [In] int dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] DuplicateHandleOptions dwOptions);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(
            [In] IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int QueryDosDevice(
            [In] string lpDeviceName,
            [Out] StringBuilder lpTargetPath,
            [In] int ucchMax);
    }
}