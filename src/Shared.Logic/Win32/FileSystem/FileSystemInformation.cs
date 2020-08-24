using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using FileLockedBy.Win32.Entities;
using FileLockedBy.Win32.Enums;
using FileLockedBy.Win32.Structures;
using FileLockedBy.Win32.System;

namespace FileLockedBy.Win32.FileSystem
{
    internal static class FileSystemInformation
    {
        private static Dictionary<string, string> deviceMap;
        private const int MaxPathLength = 260;
        private const string NetworkDevicePrefix = "\\Device\\LanmanRedirector\\";
        private static readonly int FirstHandleOffset = Marshal.OffsetOf(typeof(SystemHandlesInformation), "FirstHandle").ToInt32();
        private static readonly int SystemHandleEntrySize = Marshal.SizeOf(typeof(SystemHandleEntry));

        public static bool GetFileNameFromHandle(IntPtr handle, int processId, out string fileName)
        {
            Process currentProcess = Process.GetCurrentProcess();

            bool remote = (processId != currentProcess.Id);

            SafeProcessHandle remoteProcessHandle = null;

            SafeObjectHandle objectHandle = null;

            try
            {
                if (remote)
                {
                    remoteProcessHandle = NativeMethods.OpenProcess(ProcessAccessRights.DuplicateHandle, true, processId);

                    if (NativeMethods.DuplicateHandle(remoteProcessHandle.DangerousGetHandle(), handle, currentProcess.Handle,
                                                      out objectHandle, 0, false,
                                                      DuplicateHandleOptions.SameAccess))
                    {
                        handle = objectHandle.DangerousGetHandle();
                    }
                }
                return GetFileNameFromHandle(handle, out fileName);
            }

            finally
            {
                if (remote)
                {
                    if (remoteProcessHandle != null)
                    {
                        remoteProcessHandle.Close();
                    }

                    if (objectHandle != null)
                    {
                        objectHandle.Close();
                    }
                }
            }
        }

        public static bool GetFileNameFromHandle(IntPtr handle, out string fileName)
        {
            int[] length = {0x2000}; // 512 bytes

            using (SmartPtr sptr = new SmartPtr())
            {
                sptr.Allocate(length[0]);
                HandleFlag flag;
                if (!NativeMethods.GetHandleInformation(handle, out flag))
                {
                    fileName = null;
                    return false;
                }
                if (flag != HandleFlag.None)
                {
                    fileName = null;
                    return false;
                }
                NtStatus ret = default(NtStatus);
                ThreadStart threadStart = delegate
                                              {
                                                  ret = NativeMethods.NtQueryObject(handle, ObjectInformationClass.ObjectNameInformation,
                                                                                             sptr.Pointer, length[0], out length[0]);
                                              };
                Thread thread = new Thread(threadStart);
                thread.Start();
                bool result = thread.Join(100);
                if (!result)
                {
                    fileName = null;
                    return false;
                }
                if (ret == NtStatus.BufferOverflow)
                {
                    sptr.ReAllocate(length[0]);
                    ret = NativeMethods.NtQueryObject(handle, ObjectInformationClass.ObjectNameInformation, sptr.Pointer,
                                                      length[0], out length[0]);
                }

                if (ret == NtStatus.Success)
                {
                    ObjectNameInformation oti =
                        (ObjectNameInformation)Marshal.PtrToStructure(sptr.Pointer, typeof(ObjectNameInformation));
                    UnicodeString unicodeName = oti.Name;
                    fileName = unicodeName.GetValue();
                    return fileName.Length != 0;
                }
            }
            fileName = string.Empty;

            return false;
        }
        
        private static string NormalizeDeviceName(string deviceName)
        {
            if (string.Compare(deviceName, 0,
                               NetworkDevicePrefix, 0,
                               NetworkDevicePrefix.Length,
                               StringComparison.InvariantCulture
                    ) == 0
                )
            {
                string shareName = deviceName.Substring(
                    deviceName.IndexOf('\\', NetworkDevicePrefix.Length) + 1
                    );
                return string.Concat(NetworkDevicePrefix, shareName);
            }
            return deviceName;
        }
        
        private static Dictionary<string, string> BuildDeviceMap()
        {
            string[] logicalDrives = Environment.GetLogicalDrives();
            Dictionary<string, string> localDeviceMap = new Dictionary<string, string>(logicalDrives.Length);
            StringBuilder lpTargetPath = new StringBuilder(MaxPathLength);

            foreach (string drive in logicalDrives)
            {
                string lpDeviceName = drive.Substring(0, 2);
                NativeMethods.QueryDosDevice(lpDeviceName, lpTargetPath, MaxPathLength);
                localDeviceMap.Add(NormalizeDeviceName(lpTargetPath.ToString()), lpDeviceName);
            }

            localDeviceMap.Add(NetworkDevicePrefix.Substring(0, NetworkDevicePrefix.Length - 1), "\\");
            return localDeviceMap;
        }

        private static void EnsureDeviceMap()
        {
            if (deviceMap == null)
            {
                Dictionary<string, string> localDeviceMap = BuildDeviceMap();
                Interlocked.CompareExchange(ref deviceMap, localDeviceMap, null);
            }
        }

        private static bool ConvertDevicePathToDosPath(string devicePath, out string dosPath)
        {
            EnsureDeviceMap();
            int i = devicePath.Length;
            while (i > 0 && (i = devicePath.LastIndexOf('\\', i - 1)) != -1)
            {
                string drive;
                if (deviceMap.TryGetValue(devicePath.Substring(0, i), out drive))
                {
                    dosPath = string.Concat(drive, devicePath.Substring(i));
                    return dosPath.Length != 0;
                }
            }
            dosPath = string.Empty;
            return false;
        }

        public static FileSystemInfo GetFileSystemInfo(string devicePath)
        {
            string dosPath;
            if (ConvertDevicePathToDosPath(devicePath, out dosPath))
            {
                if (File.Exists(dosPath))
                {
                    return new FileInfo(dosPath);
                }
                if (Directory.Exists(dosPath))
                {
                    return new DirectoryInfo(dosPath);
                }
            }
            return null;
        }
        
        public static IEnumerator<FileHandleInfo> GetOpenFileHandleEnumerator(int processId, SmartPtr systemHandleInformationPointer, int handleCount)
        {
            //using (SmartPtr sptr = SystemInformation.GetSystemHandleInformation())
            //{
                //var information = (SystemHandlesInformation)Marshal.PtrToStructure(sptr.Pointer, typeof(SystemHandlesInformation));
                //int handleCount = information.Count;
                //int handleCount = Marshal.ReadInt32(sptr.Pointer);
                int offset = FirstHandleOffset;
                for (int i = 0; i < handleCount; i++, offset += SystemHandleEntrySize)
                {
                    //var entry = (SystemHandleEntry)Marshal.PtrToStructure((IntPtr) ((long) sptr.Pointer + offset), typeof (SystemHandleEntry));
                    var entry = (SystemHandleEntry)Marshal.PtrToStructure((IntPtr) (systemHandleInformationPointer.Pointer + offset), typeof (SystemHandleEntry));
                    IntPtr handle = (IntPtr) entry.HandleValue;
                    SystemHandleType handleType;
                    string devicePath;

                    if (entry.OwnerPid != processId)
                    {
                        continue;
                    }
                    if (!HandleInformation.GetHandleType(handle, processId, out handleType))
                    {
                        continue;
                    }
                    if (handleType != SystemHandleType.File)
                    {
                        continue;
                    }
                    devicePath = null;
                    if (!GetFileNameFromHandle(handle, processId, out devicePath))
                    {
                        continue;
                    }

                    FileSystemInfo info = GetFileSystemInfo(devicePath);
                    if (info != null)
                    {
                        yield return new FileHandleInfo(handle, info);
                        ;
                    }
                }
            //}
        }
    }
}